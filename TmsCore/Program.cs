using System.Diagnostics;
using System.Security.Cryptography;
// Exercise 1 — Null Safety

string? region = null;

// Null-conditional operator
string? upperRegion = region?.ToUpper();
Console.WriteLine($"Region (conditional): {upperRegion}");

// Null-coalescing operator
string displayRegion = region ?? "Unassigned";
Console.WriteLine($"Region (coalesced): {displayRegion}");

// Null-coalescing assignment
region ??= "Addis Ababa";
Console.WriteLine($"Region (assigned): {region}");

Console.WriteLine();

// TMS Variables

string studentName = "Abeba";
string studentId = "STU-001";
int enrollmentCount = 3;
decimal grantAmount = 1999.99m;
DateTime enrolledAt = DateTime.UtcNow;
string? campusRegion = null;

Console.WriteLine($"Student: {studentName} ({studentId})");
Console.WriteLine($"Courses: {enrollmentCount}");
Console.WriteLine($"Grant: {grantAmount:F2}");
Console.WriteLine($"Enrolled: {enrolledAt:yyyy-MM-dd}");
Console.WriteLine($"Campus: {campusRegion ?? "Not assigned"}");

Console.WriteLine();


// Exercise 2 — Primitives

decimal grantPerStudent = 1999.99m;
decimal totalAllocation = grantPerStudent * 100_000m;

Console.WriteLine($"Total allocated (decimal): {totalAllocation}");
Console.WriteLine($"Total allocated (formatted): {totalAllocation:F2}");

Console.WriteLine();


// Exercise 3 — Records

var enrollment = new EnrollmentRecord(
    "STU-001",
    "CS-401",
    DateTime.UtcNow
);

Console.WriteLine(enrollment);

// Non-destructive copy
var corrected = enrollment with { CourseCode = "CS-402" };

Console.WriteLine(corrected);

// Value equality
var duplicate = new EnrollmentRecord(
    "STU-001",
    "CS-401",
    enrollment.EnrolledAt
);

Console.WriteLine($"Same data? {enrollment == duplicate}");

Console.WriteLine();


// Exercise 3 Part 2 — Course Validation

var course = new Course
{
    Code = "CS-401",
    Title = "Advanced C#",
    Capacity = 30
};

Console.WriteLine($"Course: {course.Title} (Capacity: {course.Capacity})");

// Invalid capacity
try
{
    course.Capacity = -5;
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Caught: {ex.Message}");
}

// Invalid title
try
{
    course.Title = "";
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Caught: {ex.Message}");
}

Console.WriteLine();


// Exercise 3 Part 3 — Student Model

var s = new Student
{
    Id = "S1",
    Name = "Abeba",
    Age = 20,
    GPA = 3.8m
};

Console.WriteLine($"Student: {s.Name}, GPA: {s.GPA}");

Console.WriteLine();


// Exercise 3B — Interface Contracts

void PrintGradeReport(IEnumerable<IGradable> assessments)
{
    Console.WriteLine("--- Grade Report ---");

    foreach (var item in assessments)
    {
        Console.WriteLine($"{item.Title}: {item.CalculateGrade():F2}%");
    }
}

IGradable[] cohortAssessments =
[
    new Quiz
    {
        Title = "C# Basics",
        CorrectAnswers = 18,
        TotalQuestions = 20
    },

    new LabAssignment
    {
        Title = "Registration API",
        FunctionalityScore = 90m,
        CodeQualityScore = 85m
    }
];

PrintGradeReport(cohortAssessments);


//  Test 1: Valid registration

var service = new EnrollmentService();
var validStudent = new Student {Id="S1", Name = "Abeba", Age=20,GPA=3.8m};
var validCourse = new Course {Code="CS-401",Title = "Advanced C#", Capacity =30};
var result = service.ProcessRegistration(validStudent, validCourse);
Console.WriteLine($"Enrolled: {result.StudentId} in {result.CourseCode}");


//  Test 2 Null student should throw

try
{
    service.ProcessRegistration(null, validCourse);
}
catch(ArgumentNullException ex)
{
    Console.WriteLine($"Guard caught: {ex.ParamName}");
}


//  Test 3: Full course Should Throw

var fullCourse = new Course{Code="CS-402", Title="Full Course", Capacity=1};
fullCourse.EnrolledCount = 1;
try
{
    service.ProcessRegistration(validStudent, fullCourse);
}
catch(InvalidOperationException ex)
{
    Console.WriteLine($"Business rule: {ex.Message}");
}

// C# 12+ Collection Expressions  the modern way to initialize lists 

List<Student> students = [ 
    new Student { Id = "S1", Name = "Abeba", Age = 22, GPA = 3.8m }, 
    new Student { Id = "S2", Name = "Kidane", Age = 21, GPA = 2.4m }, 
    new Student { Id = "S3", Name = "Dawit", Age = 20, GPA = 3.1m }, 
    new Student { Id = "S4", Name = "Sara", Age = 23, GPA = 3.9m }, 
    new Student { Id = "S5", Name = "Frehiwot", Age = 19, GPA = 2.0m }, 
    new Student { Id = "S6", Name = "Yonas", Age = 24, GPA = 3.5m }, 
    new Student { Id = "S7", Name = "Meron", Age = 22, GPA = 1.8m }, 
    new Student { Id = "S8", Name = "Tesfaye", Age = 21, GPA = 2.9m } 
]; 

var leaderboard = students 
    // TODO 1: Extract students where GPA is >= 3.5m 
    .Where(s=> s.GPA >= 3.5m)
    // TODO 2: Sort the remaining students by GPA descending 
    .OrderByDescending(s => s.GPA)
    // TODO 3: Project the result so we only keep the 'Name' string 
    .Select(s => s.Name)
    // TODO 4: Materialize the lazy query into a concrete List 
    .ToList(); 
 
Console.WriteLine($"Found {leaderboard.Count} Honors Students:"); 
foreach (var name in leaderboard) 
{ 
    Console.WriteLine($"- {name}"); 
}

// TODO 5: Use LINQ to calculate the average GPA across all students. 
//         Format it to 2 decimal places using :F2. 
decimal averageGpa = students.Where(s=>s.GPA>=3.5m).Average(s => s.GPA); 
// Stuck? Pattern: students.Average(s => s.SomeProperty) 
Console.WriteLine($"\nClass Average GPA: {averageGpa:F2}"); 

// TODO 6: Use .GroupBy with a switch expression to classify each student. 
//         GPA >= 3.5 → "Honors", >= 2.5 → "Good Standing", 
//         >= 2.0 → "Probation", < 2.0 → "Academic Warning" 
var standingGroups = students.GroupBy(s => s.GPA switch
{
    >= 3.5m => "Honors",
    >=2.5m=>"Good Standing",
    >=2.0m=>"Probation",
    _=>"Acadamic Warning"
}); 
// Stuck? Pattern: .GroupBy(s => s.GPA switch { >= X => "Label", ... }) 
Console.WriteLine("\n--- Academic Standing Report ---"); 
foreach (var group in standingGroups) 
{ 
Console.WriteLine($"\n{group.Key} ({group.Count()}):"); 
foreach (var stud in group) 
{ 
Console.WriteLine($"  {stud.Name}  GPA: {stud.GPA}"); 
} 
}
// TODO 7: Use the spread operator (..) to merge two arrays and append a value. 
// Stuck? Pattern: string[] combined = [..array1, ..array2, "extra"]; 
string[] backendCourses = ["C#", "ASP.NET Core"]; 
string[] frontendCourses = ["TypeScript", "Angular"]; 
string[] allCourses = [..backendCourses,..frontendCourses,"extra"];// TODO 
Console.WriteLine($"\nFull curriculum: {string.Join(", ", allCourses)}");

//The wrong way; blocking with Thread.Sleep

var sw = Stopwatch.StartNew();
for(int i=0; i<5; i++)
{
    Thread.Sleep(300); //Thread is held for 300ms can not serve anyone else
}
Console.WriteLine($"Blocking Sequential:{sw.ElapsedMilliseconds}ms");

//Async but still sequential: Thread realese but calls are one at a time

sw.Restart();
for(int i = 0; i < 5; i++)
{
    await Task.Delay(300);
}
Console.WriteLine($"Async seqential: {sw.ElapsedMilliseconds}ms");


//The Right Way Async parallel all 5 start simulyaneously

sw.Restart();
var tasks = Enumerable.Range(0,5).Select(_=>Task.Delay(300));
await Task.WhenAll(tasks);
Console.WriteLine($"Async parallel: {sw.ElapsedMilliseconds}ms");

//Build TMS student Fetcher

async Task<Student>FetchStudentAsync(string id)
{
    Console.WriteLine($"\nFetching {id}...");

await Task.Delay(300);
return new Student
{
    Id=id,
    Name=$"Student-{id}",
    Age=20,
    GPA=id switch
    {
        "S1"=>3.8m,
        "S2"=>2.4m,
        "S3"=>3.5m,
        "S4"=>1.9m,
        "S5"=>3.2m,
        _=>2.5m
    }
};
}

//second method fetches course
async Task<Course>FetchCourseAsync(string code)
{
    Console.WriteLine($"Fetching Course {code}...");
    await Task.Delay(200);
    return new Course
    {
        Code = code,
        Title= $"Course-{code}",
        Capacity =code switch
        {
            "CRS-101"=>2,
            "CRS-201"=>30,
            "CRS-301"=>15,
            _=>25
        }
    };
}

sw.Restart();

string[] studentIds = {"S1","S2","S3","S4","S5"};
string[] courseCodes = {"CRS-101","CRS-201","CRS-301"};

var studentTasks = studentIds.Select(id=>FetchStudentAsync(id));
var courseTasks = courseCodes.Select(code=>FetchCourseAsync(code));

//both arrays load concurrently
Student[] studentes=await Task.WhenAll(studentTasks);
Course[] courses=await Task.WhenAll(courseTasks);

Console.WriteLine($"\nLoaded {studentes.Length} students and {courses.Length} courses in {sw.ElapsedMilliseconds}ms");

foreach(var a in studentes){
    Console.WriteLine($"{a.Name} GPA: {a.GPA}");
}

//Excersise Part 6B

var enrollCourse = new Course{Code="CRS-101", Title="C# Mastery", Capacity = 2};
var enrollService = new EnrollmentService();

var enrollments = new List<EnrollmentRecord>();
var failures = new List<string>();

sw.Restart();

foreach (var student in studentes)
{
    try
    {
        var record = enrollService.ProcessRegistration(
            student,
            enrollCourse
        );

        enrollCourse.EnrolledCount++;

        enrollments.Add(record);

        Console.WriteLine($"Enrolled: {student.Name}");
    }
    catch (CapacityReachedException ex)
    {
        failures.Add(
            $"{student.Name}: {ex.Message}"
        );

        Console.WriteLine(
            $"Rejected: {student.Name} {ex.Message}"
        );
    }
}

Console.WriteLine();

// ====================================
// Exercise 7 — Custom Exceptions
// ====================================

try
{
    var overflowCourse = new Course
    {
        Code = "CRS-999",
        Title = "Overflow Test",
        Capacity = 0
    };

    enrollService.ProcessRegistration(
        new Student
        {
            Id = "S99",
            Name = "Test",
            Age = 20,
            GPA = 3.0m
        },
        overflowCourse
    );
}
catch (CapacityReachedException ex)
{
    Console.WriteLine("\nDomain exception caught:");

    Console.WriteLine($"Course: {ex.CourseCode}");

    Console.WriteLine($"Message: {ex.Message}");
}

Console.WriteLine();


// ====================================
// Exercise 7B — Enrollment Summary
// ====================================

sw.Stop();

decimal classAverage =
    studentes.Length > 0
        ? studentes.Average(s => s.GPA)
        : 0m;

Console.WriteLine(
    "\n========== ENROLLMENT SUMMARY =========="
);

Console.WriteLine(
    $"Total students loaded: {studentes.Length}"
);

Console.WriteLine(
    $"Successful enrollments: {enrollments.Count}"
);

Console.WriteLine(
    $"Failed enrollments: {failures.Count}"
);

Console.WriteLine(
    $"Class average GPA: {classAverage:F2}"
);

Console.WriteLine(
    $"Total elapsed time: {sw.ElapsedMilliseconds}ms"
);

if (failures.Count > 0)
{
    Console.WriteLine("\n--- Failure Details ---");

    foreach (var failure in failures)
    {
        Console.WriteLine(failure);
    }
}

Console.WriteLine(
    "========================================"
);


// ====================================
// Optional — Delegates & Lambdas
// ====================================

enrollService.Listener = s =>
{
    Console.WriteLine(
        $"SMS SENT: Welcome to the TMS, {s.Name}!"
    );
};

enrollService.FinalizeEnrollment(
    new Student
    {
        Id = "S100",
        Name = "Abeba",
        Age = 20,
        GPA = 3.7m
    }
);