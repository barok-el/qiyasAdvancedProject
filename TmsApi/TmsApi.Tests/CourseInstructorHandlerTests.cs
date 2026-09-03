using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Tms.Api.Authorization;
using TmsApi.Domain.Entities;

namespace TmsApi.Tests;

public class CourseInstructorHandlerTests
{
    [Fact]
    public async Task Instructor_CanEditAssignedCourse()
    {
        var context = CreateContext("instructor-a", "Instructor", "instructor-a");

        await new CourseInstructorHandler().HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Instructor_CannotEditAnotherInstructorsCourse()
    {
        var context = CreateContext("instructor-a", "Instructor", "instructor-b");

        await new CourseInstructorHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Admin_CanEditAnyCourse()
    {
        var context = CreateContext("admin-a", "Admin", "instructor-b");

        await new CourseInstructorHandler().HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateContext(
        string userId,
        string role,
        string instructorId) =>
        new(
            [new CourseInstructorRequirement()],
            new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, role)],
                "test")),
            new Course
            {
                Code = "CSE-101",
                Title = "Test Course",
                MaxCapacity = 20,
                InstructorId = instructorId
            });
}
