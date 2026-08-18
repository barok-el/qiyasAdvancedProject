import { ApplicationConfig, provideZonelessChangeDetection } from "@angular/core";
import { provideRouter, withComponentInputBinding } from "@angular/router";
import { routes } from "./app.routes";
import {
  provideHttpClient,
  withInterceptors,
  withXsrfConfiguration
} from '@angular/common/http';

import { credentialsInterceptor } from './Interceptors/credentials.interceptor';
import { errorInterceptor } from "./Interceptors/error.interceptor";
export const appConfig: ApplicationConfig = {
providers: [
provideZonelessChangeDetection(),
provideRouter(routes, withComponentInputBinding()),
provideHttpClient(
  withInterceptors([
    credentialsInterceptor,
    errorInterceptor
  ]),
  withXsrfConfiguration({
    cookieName: 'XSRF-TOKEN',
    headerName: 'X-XSRF-TOKEN'
  })
),
],
};