namespace TmsApi.Application.Dtos;

public record GradeRequest(
    int EnrollmentId,
    decimal Score);
