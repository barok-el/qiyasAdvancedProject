namespace TmsApi.Application.Dtos;

public record GradeRequest(
    int StudentId,
    int CourseId,
    decimal Score);