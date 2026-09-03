import { Component, computed, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../services/auth.service';
import { CourseStore } from '../../store/course.store';
import { EnrollmentStore } from '../../store/enrollment.store';

@Component({
  selector: 'tms-admin-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly courseStore = inject(CourseStore);
  readonly enrollmentStore = inject(EnrollmentStore);
  private readonly router = inject(Router);

  readonly studentCount = computed(
    () => new Set(this.enrollmentStore.entities().map(row => row.studentId)).size
  );

  ngOnInit(): void {
    this.courseStore.loadCourses();
    this.enrollmentStore.loadEnrollments('admin');
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
