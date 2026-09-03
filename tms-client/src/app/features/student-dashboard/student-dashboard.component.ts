import { Component, signal, inject, computed, OnInit } from '@angular/core';

import { Router } from '@angular/router';

import { CourseCardComponent } from '../../ui/course-card/course-card';
import { Course } from '../../models/course.model';

import { EnrollmentStore } from '../../store/enrollment.store';
import { CourseStore } from '../../store/course.store';

import { EnrollmentListComponent } from '../enrollment-list/enrollment-list.component';
import { AuthService } from '../../services/auth.service';
import { GradeRecord, GradeService } from '../../services/grade.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [
    CourseCardComponent,
    EnrollmentListComponent
  ],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.scss'
})
export class StudentDashboardComponent implements OnInit {

  private readonly router = inject(Router);

  readonly enrollmentStore = inject(EnrollmentStore);
  readonly courseStore = inject(CourseStore);
  auth = inject(AuthService);
  private readonly gradeService = inject(GradeService);
  readonly grades = signal<GradeRecord[]>([]);
  
  studentName = computed(() =>
    this.auth.currentUser()?.firstName || 'Student'
  );

  earnedCredits = signal(45);

  graduationStatus = computed(() =>
    this.earnedCredits() >= 120
      ? 'Eligible for Graduation'
      : 'In Progress'
  );

  selectedCourse = signal<Course | null>(null);

  ngOnInit() {
    this.courseStore.loadCourses();
    this.enrollmentStore.loadEnrollments('student');
    this.gradeService.getMine().subscribe({ next: grades => this.grades.set(grades) });
  }

  handleEnroll(course: Course) {
    this.selectedCourse.set(course);

    console.log(
      'Enrollment requested for:',
      course.title
    );

    this.router.navigate(['/enroll'], {
      queryParams: {
        courseCode: course.code
      }
    });
  }

  handleDelete(course: Course) {
    this.courseStore.deleteCourse(course.id);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

}
