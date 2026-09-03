namespace TmsApi.Application.Dtos;

public sealed record InstructorListItemDto(
    string Id,
    string DisplayName,
    string? Email);
