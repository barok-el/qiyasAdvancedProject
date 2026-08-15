using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/grades")]
public class GradesController(TmsDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SubmitGrade(
        [FromBody] GradeRequest request,
        CancellationToken ct)
    {
        if (request.StudentId <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid student ID",
                Detail = "Student ID must be greater than zero.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (request.CourseId <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid course ID",
                Detail = "Course ID must be greater than zero.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (request.Score < 0 || request.Score > 100)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid score",
                Detail = "Score must be between 0 and 100.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var enrollment = await context.Enrollments
            .FirstOrDefaultAsync(
                e =>
                    e.StudentId == request.StudentId &&
                    e.CourseId == request.CourseId,
                ct);

        if (enrollment is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Enrollment not found",
                Detail =
                    $"Student {request.StudentId} is not enrolled in course {request.CourseId}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        enrollment.Grade = request.Score;

        await context.SaveChangesAsync(ct);

        return Ok(new GradeResponse(
            enrollment.Id.ToString(),
            true));
    }
}