using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/grades")]
[ApiVersion("2.0")]
public class GradesController(TmsDbContext context) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await ProjectGrades(context.Enrollments).ToListAsync(ct));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var student = await GetCurrentStudentAsync(ct);
        if (student is null)
            return Forbid();

        return Ok(await ProjectGrades(context.Enrollments
            .Where(item => item.StudentId == student.Id))
            .ToListAsync(ct));
    }

    [Authorize(Roles = "Instructor")]
    [HttpGet("instructor")]
    public async Task<IActionResult> GetForInstructor(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Forbid();

        return Ok(await ProjectGrades(context.Enrollments
            .Where(item => item.Course.InstructorId == userId))
            .ToListAsync(ct));
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost]
    public async Task<IActionResult> SubmitGrade(
        [FromBody] GradeRequest request,
        CancellationToken ct)
    {
        if (request.EnrollmentId <= 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid enrollment ID",
                Detail = "Enrollment ID must be greater than zero.",
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
            .Include(item => item.Course)
            .FirstOrDefaultAsync(item => item.Id == request.EnrollmentId, ct);

        if (enrollment is null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var canManage = User.IsInRole("Admin") ||
            (User.IsInRole("Instructor") && enrollment.Course.InstructorId == userId);

        if (!canManage)
            return Forbid();

        enrollment.Grade = request.Score;
        await context.SaveChangesAsync(ct);

        return Ok(new GradeResponse(enrollment.Id.ToString(), true));
    }

    private static IQueryable<GradeRecordDto> ProjectGrades(
        IQueryable<TmsApi.Domain.Entities.Enrollment> enrollments) =>
        enrollments
            .AsNoTracking()
            .OrderByDescending(item => item.EnrolledAt)
            .Select(item => new GradeRecordDto(
                item.Id,
                item.StudentId,
                item.Student.Name,
                item.CourseId,
                item.Course.Code,
                item.Course.Title,
                item.Grade));

    private async Task<TmsApi.Domain.Entities.Student?> GetCurrentStudentAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is null
            ? null
            : await context.Students.SingleOrDefaultAsync(student => student.UserId == userId, ct);
    }
}
