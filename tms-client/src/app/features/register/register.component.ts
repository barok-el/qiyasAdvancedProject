import { Component, inject, signal } from '@angular/core';
import { AbstractControl, ReactiveFormsModule, FormBuilder, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthService } from '../../services/auth.service';

const matchingPasswords: ValidatorFn = (control: AbstractControl): ValidationErrors | null =>
  control.get('password')?.value === control.get('confirmPassword')?.value
    ? null
    : { passwordMismatch: true };

@Component({
  selector: 'tms-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(12), Validators.pattern(/[A-Z]/), Validators.pattern(/[0-9]/), Validators.pattern(/[^A-Za-z0-9]/)]],
    confirmPassword: ['', Validators.required]
  }, { validators: matchingPasswords });

  async submit(): Promise<void> {
    this.error.set('');
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    const value = this.form.getRawValue();
    try {
      await this.auth.register({
        firstName: value.firstName,
        lastName: value.lastName,
        email: value.email,
        password: value.password
      });
      await this.router.navigate(['/login'], {
        state: { registrationSuccess: true }
      });
    } catch (error: any) {
      const errors = error?.error?.errors;
      this.error.set(
        Array.isArray(errors) ? errors.join(' ') :
        error?.error?.detail || 'Registration could not be completed.'
      );
    } finally {
      this.loading.set(false);
    }
  }
}
