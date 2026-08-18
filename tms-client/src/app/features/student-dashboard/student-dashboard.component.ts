import {
  Component,
  signal,
  inject,
  computed,
  OnInit
} from '@angular/core';

import { Router } from '@angular/router';

import { CourseCardComponent } from '../../ui/course-card/course-card';
import { Course } from '../../models/course.model';

import { EnrollmentStore } from '../../store/enrollment.store';
import { CourseStore } from '../../store/course.store';

import { EnrollmentListComponent } from '../enrollment-list/enrollment-list.component';

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

  studentName = signal('Liya Kebede');

  earnedCredits = signal(45);

  graduationStatus = computed(() =>
    this.earnedCredits() >= 120
      ? 'Eligible for Graduation'
      : 'In Progress'
  );

  selectedCourse = signal<Course | null>(null);

  ngOnInit() {
    this.courseStore.loadCourses();
    this.enrollmentStore.loadEnrollments();
  }

  handleEnroll(course: Course) {
    this.selectedCourse.set(course);

    console.log(
      'Enrollment requested for:',
      course.title
    );

    this.router.navigate(['/enroll'], {
      queryParams: {
        courseId: course.id
      }
    });
  }

  handleDelete(course: Course) {
    this.courseStore.deleteCourse(course.id);
  }
}