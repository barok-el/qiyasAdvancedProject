using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Common.Interface;

public interface ICachedCourseService
{
    Task<CourseResponseDto> GetCourseAsync(
        string code,
        CancellationToken ct);

    Task<List<CourseResponseDto>> GetAllCoursesAsync(
        CancellationToken ct);

    Task InvalidateCourseCacheAsync(
        CancellationToken ct);
}