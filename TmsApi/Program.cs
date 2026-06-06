using Microsoft.AspNetCore.Authentication;


using TmsApi.Middleware;
using TmsApi.Services;
using TmsApi.Configuration;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication("Training").AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
builder.Services.AddControllers();
//builder.Services.AddHostedService<EnrollmentWorker>();
//builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/assessments/results",()=> {

    Console.WriteLine("MINIMAL API ENDPOINT CALLED");

    //Console.WriteLine("MINIMAL API ENDPOINT CALLED");

    return Results.Ok(new
    {
    message = "Placeholder assessment results"
    });
})
.RequireAuthorization();

app.MapGet("/api/enrollments/worker-smoke",
    (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();

    return Results.Ok("processed");
});

app.UseHttpsRedirection();




app.MapControllers();

app.Run();
