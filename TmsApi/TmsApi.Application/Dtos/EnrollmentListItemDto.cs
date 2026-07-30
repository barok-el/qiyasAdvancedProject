namespace TmsApi.Application.Dtos;

public sealed record EnrollmentListItemDto(
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    string Status,
    DateTime EnrolledAt);
