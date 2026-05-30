import {Temporal} from "@js-temporal/polyfill";
export interface EnrollmentRecord{
    readonly studentId: string;
    readonly courseCode: string;
    enrolledAt?: Temporal.Instant;
}
export type EnrollmentStatus =|{
    status:"PENDING";
    requestedAt: Temporal.Instant;
    studentId: string;
    courseId: string;
}
|{status: "APPROVED"; approvedBy: string; approvedAt:Temporal.Instant}
|{status: "ACTIVE"; startDate: Temporal.PlainDate; currentGrade?: number}
|{status:"COMPLETED"; finalGrade: number; compeletedAt:Temporal.Instant}
|{status:"DROPPED"; reason: string; droppedAt:Temporal.Instant};

export function describeEnrollment(enrollment:EnrollmentStatus): string{
    switch (enrollment.status){
        case "PENDING":
            return`awaiting approval since ${enrollment.requestedAt}`;
        case "APPROVED":
            return `Approved by ${enrollment.approvedBy}`;
        case "ACTIVE":
            return enrollment.currentGrade !== undefined
            ?`In progress grade so far : ${enrollment.currentGrade}`
            :`In progress not yet greaded`;
        case "COMPLETED":
            return `Finished with ${enrollment.finalGrade}`;
        case "DROPPED":
            return `dropped: ${enrollment.reason}`;
        default:{
            const _check : never = enrollment;
            throw new Error(`Unhandeled status: ${JSON.stringify(_check)}`);
        }
    }
}

const pending : EnrollmentStatus = {
    status: "PENDING",
    requestedAt : Temporal.Now.instant(),
    studentId: "STU_001",
    courseId: "CRS-101",
};

console.log(describeEnrollment(pending));