import { Component, inject, signal } from "@angular/core";
import { ActivatedRoute } from '@angular/router';
import { EnrollmentService } from '../../services/enrollment.service';
import {
FormBuilder,
FormControl,
Validators,
ReactiveFormsModule,
FormArray,
} from "@angular/forms";

@Component({
  selector: 'app-enrollment-form',
  imports: [ReactiveFormsModule],
  templateUrl: './enrollment-form.component.html',
  styleUrl: './enrollment-form.component.scss',
})
export class EnrollmentFormComponent {

  private fb = inject(FormBuilder);
  private readonly enrollmentService = inject(EnrollmentService);
  submitted = signal(false);
  error = signal<string | null>(null);
  form = this.fb.nonNullable.group({
  courseCode: ["", Validators.required],
  term: ["Fall 2026", Validators.required], // Pre-filled with a default term
  notes: [""], // No validators this field is optional
  backupCourses: this.fb.array<FormControl<string>>([]), 
  });
  get backups() {
    return this.form.controls.backupCourses;
  }
  addBackup() {
    this.backups.push(
    this.fb.control("", {
    nonNullable: true,
    validators: Validators.required,
    }),
    );
  }
  removeBackup(index: number) {
    this.backups.removeAt(index);
    }
    submit() {
    if (this.form.valid) {
      this.error.set(null);
      this.enrollmentService.enroll(this.form.controls.courseCode.value).subscribe({
        next: () => this.submitted.set(true),
        error: () => this.error.set('Enrollment could not be submitted. The course may be full or already requested.')
      });
    } 
    else {
      this.form.markAllAsTouched();
    }
  }
  private route = inject(ActivatedRoute);

    constructor() {
      const courseCode = this.route.snapshot.queryParamMap.get('courseCode');

      if (courseCode) {
        this.form.controls.courseCode.setValue(courseCode);
      }
    }

}
