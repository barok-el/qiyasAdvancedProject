import{Temporal} from "@js-temporal/polyfill";
export interface Course{
    readonly id: string;
    title:string;
    capacity: number;
    startDate: Temporal.PlainDate;
}

export type CourseStatus =
     | { status: "DRAFT"; createdBy: string; createdAt: Temporal.Instant }
     | { status: "PUBLISHED"; publishedAt: Temporal.Instant; syllabus: string }
     | {
         status: "ACTIVE";
         enrolledCount: number;
         startDate: Temporal.PlainDate;
        }
        |{
          status: "ARCHIVED";
          archivedAt: Temporal.Instant;
          finalEnrollmentCount: number;
         }
     | { status: "CANCELLED"; reason: string; cancelledAt: Temporal.Instant };
      export function describeCourse(course: CourseStatus): string {
 // Your switch goes here. Handle all 5 states.
        switch(course.status){
            case "DRAFT":
                return `Draft course created by ${course.createdBy} at ${course.createdAt}`;
            case "PUBLISHED":
                 return `Published on ${course.publishedAt} with syllabus: ${course.syllabus}`;
            case "ACTIVE":
                  return `Active course with ${course.enrolledCount} enrolled students. Starts on ${course.startDate}`;
            case "ARCHIVED":
                return `Archived on ${course.archivedAt}. Final enrollment count: ${course.finalEnrollmentCount}`;
            case "CANCELLED":
                return `Cancelled on ${course.cancelledAt}. Reason: ${course.reason}`;
            default: {
                 const _exhaustive: never = course;
                 return _exhaustive;
            }
        }
 // Each case should return a descriptive string using the state-specific fields.
 // Include the default/never check.
    }