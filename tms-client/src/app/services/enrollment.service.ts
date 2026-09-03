import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Enrollment } from '../models/enrollment.model';
import { environment } from '../../environments/environment';
@Injectable({ providedIn: 'root' })
export class EnrollmentService {
    private http = inject(HttpClient);
    private readonly baseUrl = `${environment.apiUrl}/enrollments`;
    getAll(): Observable<Enrollment[]> {
        return this.http.get<Enrollment[]>(this.baseUrl);
    }
    getMine(): Observable<Enrollment[]> {
        return this.http.get<Enrollment[]>(`${this.baseUrl}/me`);
    }
    getForInstructor(): Observable<Enrollment[]> {
        return this.http.get<Enrollment[]>(`${this.baseUrl}/instructor`);
    }
    enroll(courseCode: string): Observable<unknown> {
        return this.http.post(this.baseUrl, { courseCode });
    }
    approve(id: number): Observable<Enrollment> {
     return this.http.post<Enrollment>(`${this.baseUrl}/${id}/approve`, {});
    }
    reject(id: number): Observable<Enrollment> {
     return this.http.post<Enrollment>(`${this.baseUrl}/${id}/reject`, {});
    }
}
