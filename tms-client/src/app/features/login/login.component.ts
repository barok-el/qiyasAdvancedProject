import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { RouterLink } from '@angular/router';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  loading = signal(false);
  errorMessage = signal('');
  showPassword = signal(false);
  successMessage = signal(
    history.state?.registrationSuccess
      ? 'Account created successfully. You can now sign in.'
      : ''
  );

  loginForm = this.fb.nonNullable.group({
    email: [
      '',
      [
        Validators.required,
        Validators.email
      ]
    ],
    password: [
      '',
      Validators.required
    ]
  });

  async login(): Promise<void> {
    this.errorMessage.set('');

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    try {
      await this.authService.login(
        this.loginForm.getRawValue()
      );

      await this.router.navigateByUrl(
        this.authService.getDefaultRoute()
      );
    } catch (error: any) {
      console.error('Login failed:', error);

      if (error.status === 401) {
        this.errorMessage.set(
          'Invalid email or password.'
        );
      } else if (error.status === 423) {
        this.errorMessage.set(
          'Your account is temporarily locked. Please try again later.'
        );
      } else {
        this.errorMessage.set(
          'Unable to connect to the server. Please try again.'
        );
      }
    } finally {
      this.loading.set(false);
    }
  }
}
