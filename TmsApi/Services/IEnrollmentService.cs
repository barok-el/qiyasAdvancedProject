namespace TmsApi.Services;
using TmsApi.Dtos;
using TmsApi.Entities;
public interface IEnrollmentService
{
Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
Task<IEnumerable<Enrollment>> GetAllAsync(
    CancellationToken ct);
Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
}