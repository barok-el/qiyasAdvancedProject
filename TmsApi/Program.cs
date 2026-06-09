using Microsoft.AspNetCore.Authentication;
using TmsApi.Middleware;
using TmsApi.Services;
using TmsApi.Configuration;
using Scalar.AspNetCore;
using TmsApi.Exceptions;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication("Training").AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();


builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();
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

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.MapGet("/api/assessments/results",()=> {
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


app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});







app.MapControllers();

app.Run();
