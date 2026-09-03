using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public sealed record AssignCourseInstructorRequest(
    [Required] string InstructorId);
