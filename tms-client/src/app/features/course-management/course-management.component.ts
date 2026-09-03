import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Course } from '../../models/course.model';
import { AuthService } from '../../services/auth.service';
import { CourseService, InstructorOption } from '../../services/course.service';
import { CourseStore } from '../../store/course.store';

@Component({
  selector: 'tms-course-management',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './course-management.component.html',
  styleUrl: './course-management.component.scss'
})
export class CourseManagementComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly store = inject(CourseStore);
  private readonly api = inject(CourseService);
  private readonly formBuilder = inject(FormBuilder);
  readonly isAdmin = computed(() => this.auth.hasRole('Admin'));
  readonly instructors = signal<InstructorOption[]>([]);
  readonly selectedCourse = signal<Course | null>(null);
  readonly message = signal<string | null>(null);

  readonly createForm = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.pattern('^[A-Z]{3}-\\d{3}$')]],
    title: ['', [Validators.required, Validators.maxLength(200)]],
    maxCapacity: [20, [Validators.required, Validators.min(1), Validators.max(200)]]
  });
  readonly updateForm = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]]
  });
  readonly assignmentForm = this.formBuilder.nonNullable.group({
    courseId: [0, [Validators.required, Validators.min(1)]],
    instructorId: ['', Validators.required]
  });

  ngOnInit(): void {
    if (this.isAdmin()) {
      this.store.loadCourses();
      this.api.getInstructors().subscribe({ next: rows => this.instructors.set(rows) });
    } else {
      this.store.loadMyCourses();
    }
  }

  selectForEdit(course: Course): void {
    this.selectedCourse.set(course);
    this.updateForm.controls.title.setValue(course.title);
  }

  create(): void {
    if (this.createForm.invalid) return;
    this.api.create(this.createForm.getRawValue()).subscribe({
      next: () => this.finish('Course created.'),
      error: () => this.message.set('Course could not be created.')
    });
  }

  update(): void {
    const course = this.selectedCourse();
    if (!course || this.updateForm.invalid) return;
    this.api.update(course.id, this.updateForm.controls.title.value).subscribe({
      next: () => this.finish('Course updated.'),
      error: () => this.message.set('Course could not be updated.')
    });
  }

  delete(course: Course): void {
    this.api.delete(course.id).subscribe({
      next: () => this.finish('Course deleted.'),
      error: () => this.message.set('Courses with enrollment records cannot be deleted.')
    });
  }

  assignInstructor(): void {
    if (this.assignmentForm.invalid) return;
    const value = this.assignmentForm.getRawValue();
    this.api.assignInstructor(value.courseId, value.instructorId).subscribe({
      next: () => this.finish('Instructor assigned.'),
      error: () => this.message.set('Instructor assignment could not be saved.')
    });
  }

  private finish(message: string): void {
    this.message.set(message);
    this.selectedCourse.set(null);
    this.createForm.reset({ code: '', title: '', maxCapacity: 20 });
    this.assignmentForm.reset({ courseId: 0, instructorId: '' });
    this.isAdmin() ? this.store.loadCourses() : this.store.loadMyCourses();
  }
}
