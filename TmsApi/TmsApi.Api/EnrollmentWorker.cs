using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.Common.Interface;

namespace TmsApi.Services;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        using var scope = _scopeFactory.CreateScope();

        var enrollmentService =
            scope.ServiceProvider
            .GetRequiredService<IEnrollmentService>();

        // Simulate worker activity
        var enrollments = enrollmentService
            .GetAllAsync(CancellationToken.None)
            .Result;

        Console.WriteLine(
            $"Worker processed {enrollments.Count()} enrollments.");
    }
}