namespace TmsApi.Entities;
public class Assessment
{
public int Id { get; set; }
public required string Title { get; set; }
public decimal MaxScore { get; set; }
public bool IsCompleted { get; set; }
public decimal Weight { get; set; } // share of the final grade, e.g. 0.30m for 30%
// Foreign key + navigation to the owning course
public int CourseId { get; set; }
public Course Course { get; set; } = null!;

 public int StudentId { get; set; }
public required Student Student { get; set; }
}