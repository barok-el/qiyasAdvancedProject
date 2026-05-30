//console.log("Hello TypeScript");
import {Temporal} from "@js-temporal/polyfill";
import { type Student, isStudent } from "./models/student.model.js";
import { parseStudent } from "./models/student.model.js";
import { type AssessmentItem, calculateGrade } from "./models/assessment.model.js";
import { type CourseStatus, describeCourse } from "./models/course.model.js";
const student: Student={
    id:"STU-001",
    name:"Hana Tesfaye",
    enrollmentDate: Temporal.Now.instant(),
};
//student.id = "STU-999";
console.log(student.gpa?.toFixed(2));
console.log(student.gpa?.toFixed(2)??" Not yet Graded");

/*function processStudent(data:any){
    console.log("GPA: ${data.gpa.toFixed(2)}");
}*/

//import { type Student, isStudent } from "./models/student.model.js";
function processStudent(raw: unknown){
    if(isStudent(raw)){
        const gpaDisplay = raw.gpa?.toFixed(2)??"not Graded";
        console.log(`student ${raw.name}  GPA: ${gpaDisplay}`);
    }
    else{
        console.error("invalid student data recieved");
    }
}

  processStudent({id:"STU-001", name: "Hana",gpa:3.7});
  processStudent(42);

  console.log(parseStudent({ id: "STU-001", name: "Hana" }));
 // Prints a valid Student object

  try {
    const student = parseStudent({ id: 42, name: "Test" });
    console.log(student);
} catch (error) {
    console.error(
        error instanceof Error ? error.message : "Unknown error"
    );
}
 // Throws: TypeError: Expected id to be a string, received number

 const quiz: AssessmentItem = {
    id:"Quiz-001",
    kind:"quiz",
    title:"SQL Basics",
    correctAnswer:8,
    totalQuestions:10,
 };
 const lab: AssessmentItem = {
    id:"LaB-001",
    kind:"lab",
    title:"REST API Project",
    functionalityScore: 85,
    codeQualityScore:90,
 };

 console.log(`Quiz grade: ${calculateGrade(quiz)}%`);//80
 console.log(`Lab rade: ${calculateGrade(lab)}%`);//87

 interface EnrollmentBad{
    isPending: boolean;
    isApproved: boolean;
    isActive: boolean;
    isCompleted: boolean;
    isDropped: boolean;
 }
 const webDev: CourseStatus = {
    status: "ACTIVE",
    enrolledCount: 28,
    startDate: Temporal.PlainDate.from("2026-09-01"),
 };
 console.log(describeCourse(webDev));

