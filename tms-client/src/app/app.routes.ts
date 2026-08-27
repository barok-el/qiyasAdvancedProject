import { Routes } from "@angular/router";
import { LoginComponent } from './features/login/login.component';
import { UnauthorizedComponent } from "./features/unauthorized/unauthorized.component";

export const routes: Routes = [
    {
        path: "dashboard",
        loadComponent: () =>
        import("./features/student-dashboard/student-dashboard.component").then(
        (m) => m.StudentDashboardComponent,
        ),
    },
    {
        path: 'login',
        component: LoginComponent
    },

    {
    path: 'unauthorized',
    component: UnauthorizedComponent
    },

    {
        path: 'courses/:id',
        loadComponent: () => import('./features/course-detail/course-detail')
        .then((m) => m.CourseDetailComponent)
    },
    { path: "", redirectTo: "dashboard", pathMatch: "full" },
    {
        path: 'enroll',
        loadComponent: () => import('./features/enrollment-form/enrollment-form.component')
        .then(m => m.EnrollmentFormComponent)
    },
    {
        path: 'instructor',
        loadComponent: () =>
            import('./features/instructor-dashboard/instructor-dashboard.component')
            .then(m => m.InstructorDashboardComponent)
    },
    {
        path: 'enrollments',
        loadComponent: () =>
        import('./features/enrollment-list/enrollment-list.component')
        .then(m => m.EnrollmentListComponent)
    },

    {
    path: 'grade-submission',
    loadComponent: () =>
        import('./features/grade-submission/grade-submission.component')
            .then(m => m.GradeSubmissionComponent)
    }
];
