import { CourseCardComponent } from "../../ui/course-card/course-card";
import { Course } from "../../models/course.model";
import { EnrollmentStore } from '../../store/enrollment.store';
import { rxResource } from "@angular/core/rxjs-interop";
import { CourseService } from "../../services/course.service";
import { Router, RouterLink } from '@angular/router';
import { EnrollmentListComponent } from '../enrollment-list/enrollment-list.component';
import 
{ 
  Component, signal, inject, computed, OnInit } from "@angular/core";
  @Component({
    selector: 'app-student-dashboard',
    standalone: true,
    imports: [CourseCardComponent, EnrollmentListComponent],
    templateUrl: './student-dashboard.component.html',
    styleUrl: './student-dashboard.component.scss'
  })
  export class StudentDashboardComponent implements OnInit {
    private api = inject(CourseService);
    enrollmentStore = inject(EnrollmentStore);
    studentName = signal("Liya Kebede");
    earnedCredits = signal(45);
    graduationStatus = computed(() =>
      this.earnedCredits() >= 120 ? "Eligible for Graduation" : "In Progress",
    );
    coursesResource = rxResource({
     stream: () => this.api.getAll(),
    });
    //registerForClass() {
    //  this.earnedCredits.update((c) => c + 3);
   // }
    selectedCourse = signal<Course | null>(null);
    sampleCourse: Course = {
      id: 1,
      title: "Advanced Java Services",
      code: "CSE-101",
      maxCapacity: 30,
      enrollmentCount: 12,
    };
    private router = inject(Router);

    ngOnInit() {
      this.enrollmentStore.loadEnrollments();
    }

    handleEnroll(course: Course) {
      this.selectedCourse.set(course);
      console.log('Enrollment requested for:', course.title);
      this.router.navigate(['/enroll'], {
      queryParams: { courseId: course.id },
    });
    }
}
