using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interface;
using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace TmsApi.Infrastructure.Persistence;
public class EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger) : IEnrollmentService
{
public Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int
id, CancellationToken ct) =>context.Enrollments.AsNoTracking()
.Where(e => e.Id == id && e.CourseId == courseId)
.Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.
StudentId, e.EnrolledAt))
.FirstOrDefaultAsync(ct);
public async Task<EnrollmentResponseDto> CreateAsync(int courseId,
EnrollStudentRequest request, CancellationToken ct)
{
var enrollment = new Enrollment
{
    CourseId = courseId,
    StudentId = request.StudentId,
    EnrolledAt = DateTime.UtcNow
};

context.Enrollments.Add(enrollment);

await context.SaveChangesAsync(ct);

logger.LogInformation(
    "Student {StudentId} enrolled in Course {CourseId}",
    request.StudentId,
    courseId);

return (await GetByIdAsync(
    courseId,
    enrollment.Id,ct))!;
//throw new NotImplementedException();
}
public async Task<IEnumerable<Enrollment>> GetAllAsync(
    CancellationToken ct)
{
    return await context.Enrollments
        .AsNoTracking()
        .ToListAsync(ct);
}
public async Task<IEnumerable<EnrollmentResponseDto>> GetByCourseAsync(
    int courseId,
    CancellationToken ct)
{

    return await context.Enrollments
        .AsNoTracking()

        .Where(e => e.CourseId == courseId)

        .Select(e =>
            new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt
            ))

        .ToListAsync(ct);

}

    public async Task<bool> ExistsAsync(
    int studentId,
    string courseCode,
    CancellationToken ct)
{
    return await context.Enrollments
        .AnyAsync(
            e => e.StudentId == studentId &&
                 e.Course.Code == courseCode,
            ct);
}

    public async Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct)
    {
        await context.Enrollments.AddAsync(enrollment, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(
    int studentId,
    CancellationToken ct)
    {
        return await context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<EnrollmentListItemDto>> GetEnrollmentListAsync(
        CancellationToken ct) =>
        await context.Enrollments
            .AsNoTracking()
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new EnrollmentListItemDto(
                e.Id,
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Title,
                e.Status,
                e.EnrolledAt))
            .ToListAsync(ct);

    public async Task<IEnumerable<EnrollmentListItemDto>> GetEnrollmentListByStudentIdAsync(
        int studentId,
        CancellationToken ct) =>
        await context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new EnrollmentListItemDto(
                e.Id,
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Title,
                e.Status,
                e.EnrolledAt))
            .ToListAsync(ct);

    public async Task<IEnumerable<EnrollmentListItemDto>> GetEnrollmentListForInstructorAsync(
        string instructorId,
        CancellationToken ct) =>
        await context.Enrollments
            .AsNoTracking()
            .Where(e => e.Course.InstructorId == instructorId)
            .OrderByDescending(e => e.EnrolledAt)
            .Select(e => new EnrollmentListItemDto(
                e.Id,
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Title,
                e.Status,
                e.EnrolledAt))
            .ToListAsync(ct);

    public Task<Enrollment?> GetForManagementAsync(int id, CancellationToken ct) =>
        context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<EnrollmentListItemDto?> ApproveAsync(
        int id,
        CancellationToken ct)
    {
        var enrollment = await context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (enrollment is null)
            return null;

        enrollment.Status = "Approved";
        await context.SaveChangesAsync(ct);

        return new EnrollmentListItemDto(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.Student.Name,
            enrollment.CourseId,
            enrollment.Course.Title,
            enrollment.Status,
            enrollment.EnrolledAt);
    }

    public async Task<EnrollmentListItemDto?> RejectAsync(
        int id,
        CancellationToken ct)
    {
        var enrollment = await context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (enrollment is null)
            return null;

        enrollment.Status = "Rejected";
        await context.SaveChangesAsync(ct);

        return new EnrollmentListItemDto(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.Student.Name,
            enrollment.CourseId,
            enrollment.Course.Title,
            enrollment.Status,
            enrollment.EnrolledAt);
    }
}
