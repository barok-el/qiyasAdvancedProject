namespace TmsApi.Application.Dtos;

public sealed record GradeRecordDto(
    int EnrollmentId,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseCode,
    string CourseName,
    decimal? Grade);
