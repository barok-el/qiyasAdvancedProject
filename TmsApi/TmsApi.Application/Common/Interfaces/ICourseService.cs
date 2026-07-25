using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;
namespace TmsApi.Application.Common.Interface;
public interface ICourseService
{
Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
Task<bool> CodeExistsAsync(string code, CancellationToken ct);
Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest
request, CancellationToken ct);
Task<Course?> GetByCodeAsync(string courseCode, CancellationToken ct);
Task<IEnumerable<CourseResponseDto>> GetAllAsync(
    CancellationToken ct);
Task UpdateAsync(
    UpdateCourseCommand command,
    CancellationToken ct);
}