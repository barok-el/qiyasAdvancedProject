using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public sealed record EnrollCurrentStudentRequest(
    [Required] string CourseCode);
