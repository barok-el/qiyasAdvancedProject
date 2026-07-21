using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Application.Dtos;
using TmsApi.Application.Common.Interface;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService courseService,
    LinkGenerator linkGenerator)
    : ControllerBase
{


    // Session 2 Pagination Endpoint
    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResponse<CourseResponseDto>),
        StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription(
        "Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request,
        CancellationToken ct)
    {

        var result =
            await courseService.GetCoursesAsync(
                request,
                ct);


        return Ok(result);
    }




    // Session 3 HATEOAS Detail Endpoint
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(
        typeof(CourseDetailDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription(
        "Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetCourseById(
        int id,
        CancellationToken ct)
    {

        var course =
            await courseService.GetByIdAsync(
                id,
                ct);


        if (course is null)
        {
            return NotFound();
        }



        var selfLink =
            linkGenerator.GetPathByName(
                HttpContext,
                nameof(GetCourseById),
                new { id });



        var enrollmentsLink =
            linkGenerator.GetPathByAction(
                HttpContext,
                action: "GetEnrollments",
                controller: "Enrollments",
                values: new
                {
                    courseId = id
                });



        var links =
            new List<LinkDto>
            {
                new(
                    selfLink!,
                    "self",
                    "GET"
                ),

                new(
                    selfLink!,
                    "update",
                    "PUT"
                ),

                new(
                    selfLink!,
                    "delete",
                    "DELETE"
                ),

                new(
                    enrollmentsLink!,
                    "enrollments",
                    "GET"
                )
            };



        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(
                new LinkDto(
                    enrollmentsLink!,
                    "enroll",
                    "POST"
                ));
        }



        var detail =
            new CourseDetailDto
            {
                Id = course.Id,

                Code = course.Code,

                Title = course.Title,

                MaxCapacity = course.MaxCapacity,

                EnrollmentCount = course.EnrollmentCount,

                Links = links
            };



        return Ok(detail);
    }





    // Session 1 Create Course Endpoint
    [HttpPost]
    [ProducesResponseType(
        typeof(CourseResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription(
        "Creates a course with a unique code. Returns 409 if the course code already exists.")]
    public async Task<IActionResult> CreateCourse(
        CreateCourseRequest request,
        CancellationToken ct)
    {

        if(await courseService.CodeExistsAsync(
            request.Code,
            ct))
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Course code already exists",
                    Status = StatusCodes.Status409Conflict
                });
        }


        var course =
            await courseService.CreateAsync(
                request,
                ct);



        return CreatedAtAction(
            nameof(GetCourseById),
            new
            {
                id = course.Id
            },
            course);
    }

}