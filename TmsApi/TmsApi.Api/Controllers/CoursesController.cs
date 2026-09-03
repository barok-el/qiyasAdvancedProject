using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Application.Dtos;
using TmsApi.Application.Common.Interface;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using TmsApi.Infrastructure.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(
    typeof(ProblemDetails),
    StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService courseService,
    TmsDbContext context,
    IAuthorizationService authorizationService,
    LinkGenerator linkGenerator,
    UserManager<TmsUser> userManager)
    : ControllerBase
{
    private readonly TmsDbContext _context = context;
    private readonly IAuthorizationService _authorizationService =
        authorizationService;
    private readonly UserManager<TmsUser> _userManager = userManager;

    // Session 2 Pagination Endpoint
    // Session 2 Pagination Endpoint
    [HttpGet]
    [EnableRateLimiting("search")]
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

    [Authorize(Roles = "Instructor")]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyCourses(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return Forbid();

        return Ok(await courseService.GetByInstructorIdAsync(userId, ct));
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



        var links =
            new List<LinkDto>
            {
                new(
                    selfLink!,
                    "self",
                    "GET"
                )
            };

        var managedCourse = await _context.Courses.FindAsync([id], ct);
        if (managedCourse is not null &&
            (await _authorizationService.AuthorizeAsync(User, managedCourse, "CanEditCourse")).Succeeded)
        {
            links.Add(new LinkDto(selfLink!, "update", "PUT"));
        }

        if (User.IsInRole("Admin"))
        {
            links.Add(new LinkDto(selfLink!, "delete", "DELETE"));
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
    [Authorize(Roles = "Admin")]
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

    // resource based role 
    
    
        

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            var authResult = await
            _authorizationService.AuthorizeAsync(User, course, "CanEditCourse");
            if (!authResult.Succeeded)
            {
                return Forbid(); // 403 Forbidden when caller doesn't own the resource
               
            }
            course.Title = dto.Title;
            await _context.SaveChangesAsync();
            return NoContent(); 
        }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/instructor")]
    public async Task<IActionResult> AssignInstructor(
        int id,
        [FromBody] AssignCourseInstructorRequest request)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course is null)
        {
            return NotFound();
        }

        var instructor = await _userManager.FindByIdAsync(request.InstructorId);

        if (instructor is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Instructor not found",
                Detail = "The specified user does not exist.",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (!await _userManager.IsInRoleAsync(instructor, "Instructor"))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid instructor",
                Detail = "The specified user does not have the Instructor role.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        course.InstructorId = instructor.Id;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken ct)
    {
        var course = await _context.Courses
            .Include(item => item.Enrollments)
            .FirstOrDefaultAsync(item => item.Id == id, ct);

        if (course is null)
            return NotFound();

        if (course.Enrollments.Count > 0)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course cannot be deleted",
                Detail = "Courses with enrollment records cannot be deleted.",
                Status = StatusCodes.Status409Conflict
            });
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }
    
    

}
