import { HttpClient } from '@angular/common/http';
import { inject, Injectable,Service } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface GradePayload {
    enrollmentId: number;
    score: number;
}

export interface GradeRecord {
    enrollmentId: number;
    studentId: number;
    studentName: string;
    courseId: number;
    courseCode: string;
    courseName: string;
    grade: number | null;
}

//@Service()
@Injectable({
    providedIn: 'root'
})
export class GradeService {
    private http = inject(HttpClient);
    private readonly baseUrl = `${environment.apiUrl}/grades`;

    postGrade(
        payload: GradePayload
    ): Observable<{ id: string; success: boolean }> {
        return this.http.post<{ id: string; success: boolean }>(
            this.baseUrl,
            payload
        );
    }
    getMine(): Observable<GradeRecord[]> {
        return this.http.get<GradeRecord[]>(`${this.baseUrl}/me`);
    }
    getForInstructor(): Observable<GradeRecord[]> {
        return this.http.get<GradeRecord[]>(`${this.baseUrl}/instructor`);
    }
    getAll(): Observable<GradeRecord[]> {
        return this.http.get<GradeRecord[]>(this.baseUrl);
    }
}
