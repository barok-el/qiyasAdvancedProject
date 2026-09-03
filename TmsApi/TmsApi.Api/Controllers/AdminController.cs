using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Identity;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin")]
[ApiVersion("2.0")]
[Authorize(Roles = "Admin")]
public class AdminController(UserManager<TmsUser> userManager) : ControllerBase
{
    [HttpGet("instructors")]
    public async Task<IActionResult> GetInstructors()
    {
        var instructors = await userManager.GetUsersInRoleAsync("Instructor");

        return Ok(instructors
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .Select(user => new InstructorListItemDto(
                user.Id,
                $"{user.FirstName} {user.LastName}".Trim(),
                user.Email)));
    }
}
