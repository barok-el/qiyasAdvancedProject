import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TmsUser {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly base = `${environment.apiUrl}/auth`;

  // JWT access token is kept in memory.
  // Do NOT put it in localStorage.
  private accessToken = signal<string | null>(null);

  // Refresh token is also kept in memory for this lab.
  private refreshToken = signal<string | null>(null);

  currentUser = signal<TmsUser | null>(null);

  hasRole(role: string): boolean {
    const user = this.currentUser();

    return user?.roles.includes(role) ||
           user?.roles.includes('Admin') ||
           false;
  }

  async login(credentials: LoginRequest): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<LoginResponse>(
        `${this.base}/login`,
        credentials
      )
    );

    // Store tokens in memory only.
    this.accessToken.set(response.accessToken);
    this.refreshToken.set(response.refreshToken);

    // Store user information.
    this.currentUser.set({
      userId: response.userId,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
      roles: response.roles
    });
  }

  getAccessToken(): string | null {
    return this.accessToken();
  }

  async refresh(): Promise<void> {
    const token = this.refreshToken();

    if (!token) {
      throw new Error('No refresh token available.');
    }

    const response = await firstValueFrom(
      this.http.post<LoginResponse>(
        `${this.base}/refresh`,
        {
          refreshToken: token
        }
      )
    );

    // Rotation:
    // old refresh token is replaced with the new one.
    this.accessToken.set(response.accessToken);
    this.refreshToken.set(response.refreshToken);

    this.currentUser.set({
      userId: response.userId,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
      roles: response.roles
    });
  }

  logout(): void {
    // Clear tokens from memory.
    this.accessToken.set(null);
    this.refreshToken.set(null);
    this.currentUser.set(null);
  }

  isAuthenticated(): boolean {
    return this.accessToken() !== null;
  }
}