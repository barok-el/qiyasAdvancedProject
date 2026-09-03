import { Component, computed, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { EnrollmentStore } from '../../store/enrollment.store';
import { AnalyticsChartComponent } from '../../ui/analytics-chart/analytics-chart.component';
import { CourseStore } from '../../store/course.store';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'tms-instructor-dashboard',
  standalone: true,
  imports: [AnalyticsChartComponent, RouterLink],
  templateUrl: './instructor-dashboard.component.html',
  styleUrl: './instructor-dashboard.component.scss'
})
export class InstructorDashboardComponent implements OnInit {
  readonly enrollmentStore = inject(EnrollmentStore);
  readonly courseStore = inject(CourseStore);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly studentCount = computed(
    () => new Set(this.enrollmentStore.entities().map(row => row.studentId)).size
  );

  ngOnInit(): void {
    this.courseStore.loadMyCourses();
    this.enrollmentStore.loadEnrollments('instructor');
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
