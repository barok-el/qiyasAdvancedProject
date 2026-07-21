
using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;
namespace TmsApi.Application.Common.Interface;
public interface IEnrollmentService
{
Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
Task<IEnumerable<Enrollment>> GetAllAsync(
    CancellationToken ct);
Task<IEnumerable<EnrollmentResponseDto>> GetByCourseAsync(
    int courseId,
    CancellationToken ct);
Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
    Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct);
    Task AddAsync(Enrollment enrollment, CancellationToken ct);
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(
    int studentId,
    CancellationToken ct);
}