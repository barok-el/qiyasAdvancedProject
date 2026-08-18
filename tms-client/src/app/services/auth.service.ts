import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TmsUser {
  displayName: string;
  role: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly base =
    `${environment.apiUrl}/auth`;

  currentUser = signal<TmsUser | null>(null);

  hasRole(role: string): boolean {
    const user = this.currentUser();

    return user?.role === role ||
           user?.role === 'Admin';
  }

  async login(credentials: LoginRequest): Promise<void> {
    await firstValueFrom(
      this.http.post(
        `${this.base}/login`,
        credentials
      )
    );

    const user = await firstValueFrom(
      this.http.get<TmsUser>(
        `${this.base}/me`
      )
    );

    this.currentUser.set(user);
  }

  async loadCurrentUser(): Promise<void> {
    try {
      const user = await firstValueFrom(
        this.http.get<TmsUser>(
          `${this.base}/me`
        )
      );

      this.currentUser.set(user);
    } catch {
      this.currentUser.set(null);
    }
  }

  logout(): void {
    this.currentUser.set(null);
  }
}