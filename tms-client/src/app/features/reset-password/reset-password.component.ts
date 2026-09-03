import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

const matchingPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null =>
  control.get('password')?.value === control.get('confirmPassword')?.value ? null : { passwordMismatch: true };

@Component({ selector: 'tms-reset-password', standalone: true, imports: [ReactiveFormsModule, RouterLink], templateUrl: './reset-password.component.html', styleUrl: './reset-password.component.scss' })
export class ResetPasswordComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  readonly loading = signal(false); readonly completed = signal(false); readonly error = signal('');
  readonly email = this.route.snapshot.queryParamMap.get('email') ?? '';
  readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';
  readonly form = this.formBuilder.nonNullable.group({ password: ['', [Validators.required, Validators.minLength(12), Validators.pattern(/[A-Z]/), Validators.pattern(/[0-9]/), Validators.pattern(/[^A-Za-z0-9]/)]], confirmPassword: ['', Validators.required] }, { validators: matchingPasswords });
  async submit(): Promise<void> {
    this.error.set('');
    if (!this.email || !this.token) { this.error.set('This password reset link is invalid or incomplete.'); return; }
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    try { await this.auth.resetPassword(this.email, this.token, this.form.controls.password.value); this.completed.set(true); }
    catch (error: any) { const errors = error?.error?.errors; this.error.set(Array.isArray(errors) ? errors.join(' ') : error?.error?.detail || 'Password reset failed.'); }
    finally { this.loading.set(false); }
  }
}
