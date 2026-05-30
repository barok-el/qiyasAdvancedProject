/*public class EnrollmentService
{
    public EnrollmentRecord ProcessRegistration(Student? student, Course? course)
    {
        // Guard clauses

        if (student is null)
            throw new ArgumentNullException(nameof(student));

        if (course is null)
            throw new ArgumentNullException(nameof(course));

        if (course.EnrolledCount >= course.Capacity)
            throw new InvalidOperationException("Course is full.");

        // GPA classification using switch expression

        string standing = student.GPA switch
        {
            >= 3.5m => "Honors",
            >= 2.5m => "Good Standing",
            _ => "Academic Warning"
        };

        Console.WriteLine($"{student.Name} is in {standing}.");

        // Return immutable enrollment record

        return new EnrollmentRecord(
            student.Id,
            course.Code,
            DateTime.UtcNow
        );
    }

     // Optional delegate/lambda extension

    public Action<Student>? Listener { get; set; }

    public void FinalizeEnrollment(Student s)
    {
        Console.WriteLine("Persisting to database...");

        Listener?.Invoke(s);
    }
}*/

public class EnrollmentService
{
    public EnrollmentRecord ProcessRegistration(Student? student, Course? course)
    {
        // Guard clauses

        if (student is null)
            throw new ArgumentNullException(nameof(student));

        if (course is null)
            throw new ArgumentNullException(nameof(course));

        if (course.EnrolledCount >= course.Capacity)
            throw new CapacityReachedException(course.Code);

        // GPA classification

        string standing = student.GPA switch
        {
            >= 3.5m => "Honors",
            >= 2.5m => "Good Standing",
            _ => "Academic Warning"
        };

        Console.WriteLine($"{student.Name} is in {standing}.");

        // Return immutable record

        return new EnrollmentRecord(
            student.Id,
            course.Code,
            DateTime.UtcNow
        );
    }

    // Optional delegate/lambda extension

    public Action<Student>? Listener { get; set; }

    public void FinalizeEnrollment(Student s)
    {
        Console.WriteLine("Persisting to database...");

        Listener?.Invoke(s);
    }
}
