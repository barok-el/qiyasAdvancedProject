import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Course, CourseDetail, PagedResponse } from '../models/course.model';

export interface CreateCourseRequest {
  code: string;
  title: string;
  maxCapacity: number;
}

export interface InstructorOption {
  id: string;
  displayName: string;
  email: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private readonly http = inject(HttpClient);

  private readonly base = `${environment.apiUrl}/courses`;

  getAll() {
    return this.http
      .get<PagedResponse<Course>>(this.base, {
        params: {
          page: '1',
          pageSize: '50'
        }
      })
      .pipe(
        map(response => response.items)
      );
  }

  getById(id: number) {
    return this.http.get<CourseDetail>(`${this.base}/${id}`);
  }
  getMine() {
    return this.http.get<Course[]>(`${this.base}/mine`);
  }
  create(request: CreateCourseRequest) {
    return this.http.post<Course>(this.base, request);
  }
  update(id: number, title: string) {
    return this.http.put<void>(`${this.base}/${id}`, { title });
  }
  delete(id: number) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
  assignInstructor(id: number, instructorId: string) {
    return this.http.put<void>(`${this.base}/${id}/instructor`, { instructorId });
  }
  getInstructors() {
    return this.http.get<InstructorOption[]>(`${environment.apiUrl}/admin/instructors`);
  }
}
