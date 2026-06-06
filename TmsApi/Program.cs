using Microsoft.AspNetCore.Authentication;
using TmsApi.Middleware;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication("Training").AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/assessments/results",()=> {
    //Console.WriteLine("MINIMAL API ENDPOINT CALLED");
    return Results.Ok(new
    {
    message = "Placeholder assessment results"
    });
})
.RequireAuthorization();

app.UseHttpsRedirection();




app.MapControllers();

app.Run();
