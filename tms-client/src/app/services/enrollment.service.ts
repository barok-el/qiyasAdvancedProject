import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Enrollment } from '../models/enrollment.model';
@Injectable({ providedIn: 'root' })
export class EnrollmentService {
    private http = inject(HttpClient);
    private readonly baseUrl = '/api/v2/enrollments';
    getAll(): Observable<Enrollment[]> {
        return this.http.get<Enrollment[]>(this.baseUrl);
    }
    approve(id: number): Observable<Enrollment> {
     return this.http.post<Enrollment>(`${this.baseUrl}/${id}/approve`, {});
    }
}
