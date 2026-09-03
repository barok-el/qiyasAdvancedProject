using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Application.Dtos;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Common.Interface;
using TmsApi.Application.Courses.Commands;

namespace TmsApi.Infrastructure.Persistence;


public class CourseService(
    TmsDbContext context,
    ILogger<CourseService> logger)
    : ICourseService
{


    public Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct) =>

        context.Courses
        .AsNoTracking()
        .Where(c => c.Id == id)
        .Select(c => new CourseResponseDto(
            c.Id,
            c.Code,
            c.Title,
            c.MaxCapacity,
            c.Enrollments.Count))
        .FirstOrDefaultAsync(ct);





    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct)
    {

        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };


        context.Courses.Add(course);


        await context.SaveChangesAsync(ct);



        logger.LogInformation(
            "Created course {CourseId} ({Code})",
            course.Id,
            course.Code);



        return 
            (await GetByIdAsync(course.Id, ct))!;
    }





    public Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct) =>

        context.Courses
        .AsNoTracking()
        .AnyAsync(
            c => c.Code == code,
            ct);






    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {

        IQueryable<Course> query =
            context.Courses.AsNoTracking();



        if (!string.IsNullOrWhiteSpace(request.Search))
        {

            query =
                query.Where(c =>
                    EF.Functions.ILike(
                        c.Title,
                        $"%{request.Search}%")
                    ||
                    EF.Functions.ILike(
                        c.Code,
                        $"%{request.Search}%"));

        }




        // Count BEFORE paging
        var totalCount =
            await query.CountAsync(ct);





        IQueryable<Course> sortedQuery;


        switch(request.OrderBy.ToLower())
        {

            case "code":

                sortedQuery =
                    request.Descending
                    ? query.OrderByDescending(c => c.Code)
                    : query.OrderBy(c => c.Code);

                break;



            case "maxcapacity":

                sortedQuery =
                    request.Descending
                    ? query.OrderByDescending(c => c.MaxCapacity)
                    : query.OrderBy(c => c.MaxCapacity);

                break;



            case "title":

            default:

                sortedQuery =
                    request.Descending
                    ? query.OrderByDescending(c => c.Title)
                    : query.OrderBy(c => c.Title);

                break;

        }





        var items =
            await sortedQuery

            .Skip(
                (request.Page - 1)
                * request.PageSize)

            .Take(request.PageSize)

            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))

            .ToListAsync(ct);






        return new PagedResponse<CourseResponseDto>
        {
            Items = items,

            TotalCount = totalCount,

            Page = request.Page,

            PageSize = request.PageSize
        };

    }

    public Task<Course?> GetByCodeAsync(
    string courseCode,
    CancellationToken ct) =>
    context.Courses
        .AsNoTracking()
        .Include(c => c.Enrollments)
        .FirstOrDefaultAsync(
            c => c.Code == courseCode,
            ct);

    public async Task<IEnumerable<CourseResponseDto>> GetAllAsync(
    CancellationToken ct)
    {
        return await context.Courses
            .AsNoTracking()
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<CourseResponseDto>> GetByInstructorIdAsync(
        string instructorId,
        CancellationToken ct) =>
        await context.Courses
            .AsNoTracking()
            .Where(course => course.InstructorId == instructorId)
            .OrderBy(course => course.Title)
            .Select(course => new CourseResponseDto(
                course.Id,
                course.Code,
                course.Title,
                course.MaxCapacity,
                course.Enrollments.Count))
            .ToListAsync(ct);
    public async Task UpdateAsync(
    UpdateCourseCommand command,
    CancellationToken ct)
    {
        var course = await context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == command.Id,
                ct);

        if (course is null)
            throw new KeyNotFoundException(
                $"Course {command.Id} not found.");

        course.Code = command.Code;
        course.Title = command.Title;
        course.MaxCapacity = command.MaxCapacity;

        await context.SaveChangesAsync(ct);
    }
}
