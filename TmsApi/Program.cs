using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;
using TmsApi.Configuration;
using TmsApi.Exceptions;
using TmsApi.Middleware;
using TmsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Authentication & Authorization
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training",
        null);

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// ProblemDetails
builder.Services.AddProblemDetails();

// OpenAPI
builder.Services.AddOpenApi();

// Service Provider Validation
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Services
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddSingleton<EnrollmentWorker>();

// Options Pattern
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// Middleware Pipeline

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// Development Only
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Existing Session 1 Endpoint
app.MapGet("/api/assessments/results", () =>
{
    return Results.Ok(new
    {
        message = "Placeholder assessment results"
    });
})
.RequireAuthorization();

// Session 2 Worker Test
app.MapGet("/api/enrollments/worker-smoke",
    (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

// Session 3 Error Test
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});

// Controllers
app.MapControllers();

app.Run();