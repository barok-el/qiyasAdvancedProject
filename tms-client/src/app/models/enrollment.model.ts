export interface Enrollment {
id: number;
studentId: number;
studentName: string;
courseId: number;
courseName: string;
status: 'Pending' | 'Approved' | 'Rejected';
enrolledAt: string;
}
