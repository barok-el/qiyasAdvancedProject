import { CourseCardComponent } from "../../ui/course-card/course-card";
import { Course } from "../../models/course.model";
import { rxResource } from "@angular/core/rxjs-interop";
import { CourseService } from "../../services/course.service";
import { Router, RouterLink } from '@angular/router';
import 
{ 
  Component, signal, inject,computed } from "@angular/core";
  @Component({
    selector: 'app-student-dashboard',
    standalone: true,
    imports: [CourseCardComponent], // This tells Angular: "I use CourseCardComponent in my template"
    templateUrl: './student-dashboard.component.html',
    styleUrl: './student-dashboard.component.scss'
  })
  export class StudentDashboardComponent {
    private api = inject(CourseService);
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
    handleEnroll(course: Course) {
      this.selectedCourse.set(course);
      console.log('Enrollment requested for:', course.title);
      this.router.navigate(['/enroll'], {
      queryParams: { courseId: course.id },
    });
    }
    


}
