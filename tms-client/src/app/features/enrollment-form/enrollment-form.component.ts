import { Component, inject, signal } from "@angular/core";
import { ActivatedRoute } from '@angular/router';
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
  submitted = signal(false);
  form = this.fb.nonNullable.group({
  studentId: [
  "",
  [Validators.required, Validators.pattern("^STU-[0-9]{4}$")],
  ],
  courseId: ["", Validators.required],
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
      const payload = this.form.getRawValue();
      console.log("Enrollment payload:", payload);
      this.submitted.set(true);
    } 
    else {
      this.form.markAllAsTouched();
    }
  }
  private route = inject(ActivatedRoute);

    constructor() {
      const courseId = this.route.snapshot.queryParamMap.get('courseId');

      if (courseId) {
        this.form.controls.courseId.setValue(courseId);
      }
    }

}
