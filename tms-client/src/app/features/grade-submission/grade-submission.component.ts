import { Component, inject, OnInit, signal } from '@angular/core';
import {
    FormBuilder,
    ReactiveFormsModule,
    Validators
} from '@angular/forms';
import { GradeRecord, GradeService } from '../../services/grade.service';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'tms-grade-submission',
    standalone: true,
    imports: [
        ReactiveFormsModule
    ],
    templateUrl: './grade-submission.component.html'
})
export class GradeSubmissionComponent implements OnInit {
    private api = inject(GradeService);
    private fb = inject(FormBuilder);
    private auth = inject(AuthService);
    readonly records = signal<GradeRecord[]>([]);

    // Reactive Form definition with initial model values and validators
    gradeForm = this.fb.group({
        enrollmentId: [
            0,
            [Validators.required, Validators.min(1)]
        ],
        score: [
            88,
            [
                Validators.required,
                Validators.min(0),
                Validators.max(100)
            ]
        ]
    });

    isSubmitting = false;
    submissionStatus = '';

    ngOnInit(): void {
        const request = this.auth.hasRole('Admin')
            ? this.api.getAll()
            : this.api.getForInstructor();

        request.subscribe({
            next: records => this.records.set(records),
            error: () => this.submissionStatus = 'Grade records could not be loaded.'
        });
    }

    onSubmit() {
        if (this.gradeForm.valid) {
            const rawValue = this.gradeForm.getRawValue();
            this.isSubmitting = true;
            this.submissionStatus = 'Submitting grade to server...';
            this.api.postGrade({
                enrollmentId: Number(rawValue.enrollmentId),
                score: Number(rawValue.score)
            }).subscribe({
                next: result => {
                    this.isSubmitting = false;
                    this.submissionStatus = `Grade saved successfully! Record ID: ${result.id}`;
                    this.ngOnInit();
                },
                error: err => {
                    this.isSubmitting = false;
                    this.submissionStatus = `Submission failed: ${err.message || 'Server error'}`;
                }
            });
        }
    }
}
