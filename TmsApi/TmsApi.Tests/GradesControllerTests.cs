using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Api.Controllers;
using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Tests;

public class GradesControllerTests
{
    [Fact]
    public async Task Instructor_CanSubmitGrade_ForAssignedCourse()
    {
        await using var context = CreateContext();
        var enrollment = Seed(context, "instructor-a");
        var controller = CreateController(context, "instructor-a", "Instructor");

        var result = await controller.SubmitGrade(new GradeRequest(enrollment.Id, 88), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(88, (await context.Enrollments.FindAsync(enrollment.Id))!.Grade);
    }

    [Fact]
    public async Task Instructor_CannotSubmitGrade_ForAnotherInstructorsCourse()
    {
        await using var context = CreateContext();
        var enrollment = Seed(context, "instructor-b");
        var controller = CreateController(context, "instructor-a", "Instructor");

        var result = await controller.SubmitGrade(new GradeRequest(enrollment.Id, 88), CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Student_CannotSubmitGrade()
    {
        await using var context = CreateContext();
        var enrollment = Seed(context, "instructor-a");
        var controller = CreateController(context, "student-user", "Student");

        var result = await controller.SubmitGrade(new GradeRequest(enrollment.Id, 88), CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Student_SeesOnlyOwnGradeRecords()
    {
        await using var context = CreateContext();
        Seed(context, "instructor-a");
        var otherStudent = new Student { UserId = "other-user", RegistrationNumber = "TMS-2", Name = "Other", GPA = 0m };
        context.Students.Add(otherStudent);
        context.Enrollments.Add(new Enrollment { Student = otherStudent, CourseId = 1, Grade = 76 });
        await context.SaveChangesAsync();
        var controller = CreateController(context, "student-user", "Student");

        var result = await controller.GetMine(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var grades = Assert.IsAssignableFrom<IEnumerable<GradeRecordDto>>(ok.Value);
        Assert.Single(grades);
        Assert.Equal("student-user", context.Students.Single(student => student.Id == grades.Single().StudentId).UserId);
    }

    [Fact]
    public async Task Admin_CanSubmitGrade_ForAnyCourse()
    {
        await using var context = CreateContext();
        var enrollment = Seed(context, "instructor-b");
        var controller = CreateController(context, "admin-user", "Admin");

        var result = await controller.SubmitGrade(new GradeRequest(enrollment.Id, 91), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    private static TmsDbContext CreateContext() => new(
        new DbContextOptionsBuilder<TmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Enrollment Seed(TmsDbContext context, string instructorId)
    {
        var student = new Student { UserId = "student-user", RegistrationNumber = "TMS-1", Name = "Student", GPA = 0m };
        var course = new Course { Code = "CSE-101", Title = "Security", MaxCapacity = 20, InstructorId = instructorId };
        var enrollment = new Enrollment { Student = student, Course = course };
        context.Enrollments.Add(enrollment);
        context.SaveChanges();
        return enrollment;
    }

    private static GradesController CreateController(TmsDbContext context, string userId, string role) => new(context)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, role)],
                    "test"))
            }
        }
    };
}
