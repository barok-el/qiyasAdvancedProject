using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Common.Interface;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Application.Hubs;
using TmsApi.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(
    IMediator mediator,
    IEnrollmentService enrollmentService,
    IHubContext<TmsHub, ITmsHubClient> hubContext,
    TmsDbContext context) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await enrollmentService.GetEnrollmentListAsync(ct));

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var student = await GetCurrentStudentAsync(ct);
        if (student is null)
            return Forbid();

        return Ok(await enrollmentService.GetEnrollmentListByStudentIdAsync(student.Id, ct));
    }

    [Authorize(Roles = "Instructor")]
    [HttpGet("instructor")]
    public async Task<IActionResult> GetForInstructor(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Forbid();

        return Ok(await enrollmentService.GetEnrollmentListForInstructorAsync(userId, ct));
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    public async Task<IActionResult> Enroll(
        EnrollCurrentStudentRequest request,
        CancellationToken ct)
    {
        var student = await GetCurrentStudentAsync(ct);
        if (student is null)
            return Forbid();

        var command = new EnrollStudentCommand(student.Id, request.CourseCode);
        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            onSuccess: created => Created("/api/v2/enrollments/me", created),

            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" => StatusCodes.Status404NotFound,
                    "course_full" or "already_enrolled" => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest
                };

                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}");
            });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(
        int id,
        CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetForManagementAsync(id, ct);
        if (enrollment is null)
            return NotFound();

        if (!CanManage(enrollment.Course.InstructorId))
            return Forbid();

        var approvedEnrollment = await enrollmentService.ApproveAsync(id, ct);

        if (approvedEnrollment is null)
            return NotFound();

        await hubContext.Clients.All
            .ReceiveEnrollmentStatusUpdated(
                approvedEnrollment.Id.ToString(),
                approvedEnrollment.Status);

        return Ok(approvedEnrollment);
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetForManagementAsync(id, ct);
        if (enrollment is null)
            return NotFound();

        if (!CanManage(enrollment.Course.InstructorId))
            return Forbid();

        var rejectedEnrollment = await enrollmentService.RejectAsync(id, ct);
        if (rejectedEnrollment is null)
            return NotFound();

        await hubContext.Clients.All.ReceiveEnrollmentStatusUpdated(
            rejectedEnrollment.Id.ToString(),
            rejectedEnrollment.Status);

        return Ok(rejectedEnrollment);
    }

    [Authorize]
    [HttpGet("me/schedule")]
    public async Task<IActionResult> GetMySchedule(CancellationToken ct)
    {
        var student = await GetCurrentStudentAsync(ct);
        if (student is null)
            return Forbid();

        return Ok(await mediator.Send(new GetStudentScheduleQuery(student.Id), ct));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{studentId}/schedule")]
    public async Task<IActionResult> GetSchedule(
        int studentId,
        CancellationToken ct)
    {
        var schedule = await mediator.Send(
            new GetStudentScheduleQuery(studentId),
            ct);

        return Ok(schedule);
    }

    private async Task<TmsApi.Domain.Entities.Student?> GetCurrentStudentAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is null
            ? null
            : await context.Students.SingleOrDefaultAsync(student => student.UserId == userId, ct);
    }

    private bool CanManage(string? instructorId) =>
        User.IsInRole("Admin") ||
        (User.IsInRole("Instructor") &&
         instructorId == User.FindFirstValue(ClaimTypes.NameIdentifier));
}
