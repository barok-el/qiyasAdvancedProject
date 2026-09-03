import { Component, inject, input, OnInit, signal } from "@angular/core";
import { RouterLink } from '@angular/router';
import { CourseDetail } from '../../models/course.model';
import { CourseService } from '../../services/course.service';
@Component({
selector: "app-course-detail",
imports: [RouterLink],
standalone: true,
templateUrl: "./course-detail.html",
styleUrl: './course-detail.scss'
})
export class CourseDetailComponent implements OnInit {

    id = input.required<string>();
    private readonly courseService = inject(CourseService);
    readonly course = signal<CourseDetail | null>(null);
    readonly loading = signal(true);
    readonly error = signal('');

    ngOnInit(): void {
      const courseId = Number(this.id());

      if (!Number.isInteger(courseId) || courseId <= 0) {
        this.loading.set(false);
        this.error.set('The course identifier is invalid.');
        return;
      }

      this.courseService.getById(courseId).subscribe({
        next: course => {
          this.course.set(course);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Course details could not be loaded.');
          this.loading.set(false);
        }
      });
    }
}
