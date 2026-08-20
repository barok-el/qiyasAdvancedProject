using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System.Threading.RateLimiting;
using System.Threading.Channels;

using FluentValidation;
using MediatR;
using Scalar.AspNetCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using TmsApi.Api.ExceptionHandlers;
using TmsApi.Api.Filters;
using TmsApi.Api.Middleware;
using TmsApi.Api.RateLimiting;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Common.Interface;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Workers;
using TmsApi.Api.Hubs;
using TmsApi.Application.Notifications;
using TmsApi.Api.Notifications;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using TmsApi.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine(builder.Environment.EnvironmentName);
Console.WriteLine(
    builder.Configuration.GetConnectionString("TmsDatabase"));


// =======================================================
// Rate Limiting
// =======================================================

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                var (partitionKey, tier) =
                    ApiKeyResolver.Resolve(httpContext);

                return tier switch
                {
                    ApiKeyTier.Paid =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: $"paid:{partitionKey}",
                            factory: _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 200,
                                TokensPerPeriod = 100,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            }),

                    ApiKeyTier.Free =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: $"free:{partitionKey}",
                            factory: _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 30,
                                TokensPerPeriod = 10,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            }),

                    _ =>
                        RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: $"anon:{partitionKey}",
                            factory: _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 10,
                                TokensPerPeriod = 5,
                                ReplenishmentPeriod =
                                    TimeSpan.FromSeconds(10),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            })
                };
            });

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, cancellationToken) =>
    {
        var retryAfter = "unknown";

        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out var retry))
        {
            retryAfter =
                ((int)retry.TotalSeconds).ToString();
        }

        context.HttpContext.Response.Headers.RetryAfter =
            retryAfter;

        context.HttpContext.Response.ContentType =
            "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = "Rate limit exceeded",

                Detail =
                    $"Too many requests. Retry after {retryAfter} seconds.",

                Status =
                    StatusCodes.Status429TooManyRequests,

                Type =
                    "https://tms.local/errors/rate_limit_exceeded"
            },
            cancellationToken);
    };

    

    // ===============================================
    // Transcript Concurrency Policy
    // ===============================================

    options.AddConcurrencyLimiter(
        "transcripts",
        limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;

            limiterOptions.QueueLimit = 20;

            limiterOptions.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
        });

    // ===============================================
    // Search Endpoint Policy
    // ===============================================

    options.AddTokenBucketLimiter(
        "search",
        limiterOptions =>
        {
            limiterOptions.TokenLimit = 10;

            limiterOptions.TokensPerPeriod = 5;

            limiterOptions.ReplenishmentPeriod =
                TimeSpan.FromSeconds(10);

            limiterOptions.QueueLimit = 2;

            limiterOptions.AutoReplenishment = true;
        });
});

//================================================
    //XSRF token
    //================================================

    builder.Services.AddAntiforgery(options =>
    {
        options.HeaderName = "X-XSRF-TOKEN";
    });

// =======================================================
// MediatR
// =======================================================

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(EnrollStudentHandler).Assembly));

builder.Services.AddValidatorsFromAssembly(
    typeof(EnrollStudentValidator).Assembly);

// =======================================================
// In-Memory Transcript Status Store
// Shared by all requests and background workers
// =======================================================

builder.Services.AddSingleton<
    ITranscriptStatusStore,
    InMemoryTranscriptStatusStore>();

// =======================================================
// Transcript Processing Queue
// Bounded channel used to queue transcript requests
// =======================================================

builder.Services.AddSingleton(
    Channel.CreateBounded<TranscriptRequest>(
        new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        }));

// =======================================================
// Background Transcript Processing Worker
// Processes queued transcript generation requests
// =======================================================

builder.Services.AddHostedService<TranscriptWorker>();

// =======================================================
// SignalR Real-Time Communication
// =======================================================

builder.Services.AddSignalR();

// =======================================================
// Transcript Notification Service
// =======================================================

builder.Services.AddSingleton<
    ITranscriptNotificationService,
    SignalRTranscriptNotificationService>();

// =======================================================
// Pipeline Behaviors
// =======================================================

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

// =======================================================
// Exception Handling
// =======================================================

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// =======================================================
// Hybrid Cache
// =======================================================

builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions =
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10),
            LocalCacheExpiration =
                TimeSpan.FromMinutes(2)
        };
});

builder.Services.AddScoped<
    ICachedCourseService,
    CachedCourseService>();

// =======================================================
// Authentication / Authorization
// =======================================================

builder.Services.AddScoped<TokenService>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!))
            };
    });

builder.Services.AddAuthorization();

// =======================================================
// ASP.NET Core Identity
// =======================================================

builder.Services.AddIdentityCore<TmsUser>(options =>
{
    // Password Policy
    options.Password.RequiredLength = 12;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    // Brute-Force Lockout Protection
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan =
        TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<TmsDbContext>();

// =======================================================
// Application Services
// =======================================================

builder.Services.AddScoped<
    ICourseService,
    CourseService>();

builder.Services.AddScoped<
    IEnrollmentService,
    EnrollmentService>();

// =======================================================
// Controllers
// =======================================================

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});

// =======================================================
// OpenAPI
// =======================================================

builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude =
        description => description.GroupName == "v1";
});

builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude =
        description => description.GroupName == "v2";
});

// =======================================================
// API Versioning
// =======================================================

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion =
            new ApiVersion(1, 0);

        options.AssumeDefaultVersionWhenUnspecified =
            true;

        options.ReportApiVersions = true;

        options.ApiVersionReader =
            ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// =======================================================
// Health Checks
// =======================================================

builder.Services.AddHealthChecks();

// =======================================================
// DbContext
// =======================================================

builder.Services.AddDbContext<TmsDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase"));

    options.LogTo(
        Console.WriteLine,
        LogLevel.Information);

    options.EnableSensitiveDataLogging();
});

// =======================================================
// Service Provider Validation
// =======================================================

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

//=======================================================
//angular CORS
//=======================================================

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// =======================================================
// Build
// =======================================================

var app = builder.Build();





// =======================================================
// Middleware Pipeline
// =======================================================

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseRouting();

//========================================================
//Tms-Client CORs
//========================================================

app.UseCors("TmsClient");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

//=======================================================
//
//=======================================================

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true ||
        context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery =
            context.RequestServices
                .GetRequiredService<IAntiforgery>();

        var tokens =
            antiforgery.GetAndStoreTokens(context);

        context.Response.Cookies.Append(
            "XSRF-TOKEN",
            tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = !builder.Environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict
            });
    }

    await next(context);
});

// =======================================================
// Health Endpoints
// =======================================================

app.MapGet("/", () => Results.Ok(new
{
    name = "TMS API",
    status = "running",
    health = "/health/live",
    courses = "/api/v2/courses",
    enrollments = "/api/v2/enrollments"
})).DisableRateLimiting();

app.MapHealthChecks("/health/live")
    .DisableRateLimiting();

app.MapHealthChecks("/health/ready")
    .DisableRateLimiting();

// =======================================================
// CORS
// =======================================================

//app.UseCors("AllowAngular");

// =======================================================
// OpenAPI + Scalar
// =======================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TMS API Reference")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(
                ScalarTarget.CSharp,
                ScalarClient.HttpClient);

        options
            .AddDocument(
                "v1",
                "API Version 1.0")
            .AddDocument(
                "v2",
                "API Version 2.0");
    });
}

// =======================================================
// Custom Middleware
// =======================================================

app.UseMiddleware<V1DeprecationMiddleware>();

// =======================================================
// Controllers
// =======================================================

// =======================================================
// Map SignalR Hub Endpoint
// =======================================================

app.MapHub<TmsHub>("/hubs/tms")
    .RequireCors("TmsClient");

app.MapControllers();

// =======================================================
// Database Migration + Initial Seed
// =======================================================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<TmsDbContext>();

    context.Database.Migrate();

    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new()
            {
                RegistrationNumber = "TMS-2026-0001",
                Name = "Alice Smith",
                GPA = 3.8m,
                IsActive = true
            },

            new()
            {
                RegistrationNumber = "TMS-2026-0002",
                Name = "Bob Jones",
                GPA = 2.9m,
                IsActive = true
            },

            new()
            {
                RegistrationNumber = "TMS-2026-0003",
                Name = "Charlie Brown",
                GPA = 3.4m,
                IsActive = false
            },

            new()
            {
                RegistrationNumber = "TMS-2026-0004",
                Name = "Diana Prince",
                GPA = 3.9m,
                IsActive = true
            },

             new()
            {
                RegistrationNumber = "TMS-2026-0006",
                Name = "Sara Stars",
                GPA = 3.5m,
                IsActive = false
            },

            new()
            {
                RegistrationNumber = "TMS-2026-0005",
                Name = "Evan Wright",
                GPA = 2.5m,
                IsActive = true
            }
        };

        context.Students.AddRange(students);

        var courses = new List<Course>
        {
            new()
            {
                Code = "CS-101",
                Title = "Introduction to Computer Science",
                MaxCapacity = 30
            },

            new()
            {
                Code = "CS-201",
                Title = "Data Structures and Algorithms",
                MaxCapacity = 25
            },

            new()
            {
                Code = "MAT-101",
                Title = "Calculus I",
                MaxCapacity = 40
            }
        };

        context.Courses.AddRange(courses);

        context.SaveChanges();

        var enrollments = new List<Enrollment>
        {
            new()
            {
                StudentId = students[0].Id,
                CourseId = courses[0].Id,
                Grade = 4.0m
            },

            new()
            {
                StudentId = students[0].Id,
                CourseId = courses[1].Id,
                Grade = 3.6m
            },

            new()
            {
                StudentId = students[1].Id,
                CourseId = courses[0].Id,
                Grade = 2.8m
            },

            new()
            {
                StudentId = students[3].Id,
                CourseId = courses[1].Id,
                Grade = 3.9m
            },

             new()
            {
                StudentId = students[3].Id,
                CourseId = courses[1].Id,
                Grade = 2.8m
            },

             new()
            {
                StudentId = students[1].Id,
                CourseId = courses[1].Id,
                Grade = 2.8m
            }
        };

        context.Enrollments.AddRange(enrollments);

        context.SaveChanges();
    }
}

// =======================================================
// Development Seeder
// =======================================================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context =
        scope.ServiceProvider
            .GetRequiredService<TmsDbContext>();

    await DataSeeder.SeedAsync(context);
}

//=======================================================
//Test password hash
//=======================================================

var service = new CryptoDemoService();

string hash1 = service.HashUserPassword("Password123!");
string hash2 = service.HashUserPassword("Password123!");

Console.WriteLine($"Hash 1: {hash1}");
Console.WriteLine($"Hash 2: {hash2}");

bool match1 = service.VerifyUserPassword(
    "Password123!",
    hash1);

bool match2 = service.VerifyUserPassword(
    "Password123!",
    hash2);

Console.WriteLine($"Match 1: {match1}");
Console.WriteLine($"Match 2: {match2}");

// =======================================================
// Run
// =======================================================

app.Run();
