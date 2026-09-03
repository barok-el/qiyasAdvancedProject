import { Routes } from "@angular/router";
import { LoginComponent } from './features/login/login.component';
import { UnauthorizedComponent } from "./features/unauthorized/unauthorized.component";
import { roleGuard } from './guards/role.guard';

export const routes: Routes = [

    {
        path: 'login',
        component: LoginComponent
    },
    {
        path: 'register',
        loadComponent: () => import('./features/register/register.component')
            .then(m => m.RegisterComponent)
    },
    {
        path: 'forgot-password',
        loadComponent: () => import('./features/forgot-password/forgot-password.component')
            .then(m => m.ForgotPasswordComponent)
    },
    {
        path: 'reset-password',
        loadComponent: () => import('./features/reset-password/reset-password.component')
            .then(m => m.ResetPasswordComponent)
    },

    {
        path: "dashboard",
        canActivate: [roleGuard('Student')],
        loadComponent: () =>
        import("./features/student-dashboard/student-dashboard.component").then(
        (m) => m.StudentDashboardComponent,
        ),
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
        canActivate: [roleGuard('Instructor')],
        loadComponent: () =>
            import('./features/instructor-dashboard/instructor-dashboard.component')
            .then(m => m.InstructorDashboardComponent)
    },
    {
        path: 'course-management',
        canActivate: [roleGuard('Instructor')],
        loadComponent: () => import('./features/course-management/course-management.component')
            .then(m => m.CourseManagementComponent)
    },
    {
        path: 'enrollments',
        canActivate: [roleGuard('Instructor')],
        loadComponent: () =>
        import('./features/enrollment-list/enrollment-list.component')
        .then(m => m.EnrollmentListComponent)
    },

    {
    path: 'grade-submission',
    canActivate: [roleGuard('Instructor')],
    loadComponent: () =>
        import('./features/grade-submission/grade-submission.component')
            .then(m => m.GradeSubmissionComponent)
    },
    {
        path: 'admin',
        canActivate: [roleGuard('Admin')],
        loadComponent: () =>
            import('./features/admin-dashboard/admin-dashboard.component')
                .then(m => m.AdminDashboardComponent)
    },
    { path: '**', redirectTo: 'login' }
];
