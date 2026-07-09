using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TmsApi.Data;
using TmsApi.Entities; // Make sure your entities namespace is included

namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (no database contact)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);
        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);

        Console.WriteLine(">>> SQL Generated:\n" + orderedQuery.ToQueryString());

        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList(); // Execution is triggered here
        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
        return Ok(results);
    }

    // Non-translatable helper method
    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
        try
        {
            var query = context.Students.Where(s => IsHonorRoll(s.GPA));

            Console.WriteLine(">>> SQL Generated:\n" + query.ToQueryString());

            var students = query.ToList();
            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
            return BadRequest(new { Message = ex.Message });
        }
    }

    // -------------------------------
    // Registrar’s Business Queries
    // -------------------------------

    // 1. List all students enrolled in a given course
    [HttpGet("students-in-course/{courseId}")]
    public IActionResult GetStudentsInCourse(int courseId)
    {
        var query = context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.Student);

        Console.WriteLine(">>> SQL Generated:\n" + query.ToQueryString());

        var students = query.ToList();
        return Ok(students);
    }

    // 2. Find courses with no enrollments
    [HttpGet("empty-courses")]
    public IActionResult GetEmptyCourses()
    {
        var query = context.Courses
            .Where(c => !c.Enrollments.Any());

        Console.WriteLine(">>> SQL Generated:\n" + query.ToQueryString());

        var courses = query.ToList();
        return Ok(courses);
    }

    // 3. Get number of students per course
    [HttpGet("course-counts")]
    public IActionResult GetCourseCounts()
    {
        var query = context.Enrollments
            .GroupBy(e => e.Course)
            .Select(g => new { Course = g.Key.Title, Count = g.Count() });

        Console.WriteLine(">>> SQL Generated:\n" + query.ToQueryString());

        var courseCounts = query.ToList();
        return Ok(courseCounts);
    }

    // 4. Retrieve students who have completed all assessments
    [HttpGet("completed-students")]
    public IActionResult GetCompletedStudents()
    {
        var query = context.Students
            .Where(s => s.Assessments.All(a => a.IsCompleted));

        Console.WriteLine(">>> SQL Generated:\n" + query.ToQueryString());

        var students = query.ToList();
        return Ok(students);
    }

[HttpGet("students-grouped-by-gpa")]
public IActionResult GetStudentsGroupedByGpa()
{
    var query = context.Students
        .GroupBy(s => Math.Floor(s.GPA))
        .Select(g => new { GPABucket = g.Key, Count = g.Count() });

    Console.WriteLine(">>> SQL Generated:\n" + query.ToQueryString());
    return Ok(query.ToList());
}

[HttpGet("average-gpa-per-course")]
public IActionResult GetAverageGpaPerCourse()
{
    var query = context.Enrollments
        .GroupBy(e => e.Course.Title)
        .Select(g => new { Course = g.Key, AverageGPA = g.Average(e => e.Student.GPA) });

    Console.WriteLine(">>> SQL Generated:\n" + query.ToQueryString());
    return Ok(query.ToList());
}

[HttpGet("students-paged")]
public IActionResult GetPagedStudents(int pageNumber = 1, int pageSize = 10)
{
    var query = context.Students
        .OrderBy(s => s.Name)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize);

    Console.WriteLine(">>> SQL Generated:\n" + query.ToQueryString());
    return Ok(query.ToList());
}



}