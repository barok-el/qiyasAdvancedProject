using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Common.Interface;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Application.Hubs;
using TmsApi.Api.Hubs;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(
    IMediator mediator,
    IEnrollmentService enrollmentService,
    IHubContext<TmsHub, ITmsHubClient> hubContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await enrollmentService.GetEnrollmentListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Enroll(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            onSuccess: created => CreatedAtAction(
                nameof(GetSchedule),
                new { studentId = created.StudentId },
                created),

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

   [HttpPost("{id:int}/approve")]
public async Task<IActionResult> Approve(
    int id,
    CancellationToken ct)
{
    var enrollment = await enrollmentService.ApproveAsync(id, ct);

    if (enrollment is null)
        return NotFound();

    await hubContext.Clients.All
        .ReceiveEnrollmentStatusUpdated(
            enrollment.Id.ToString(),
            enrollment.Status.ToString());

    return Ok(enrollment);
}

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

}
