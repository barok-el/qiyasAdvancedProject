import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { EnrollmentStore } from './store/enrollment.store';

@Component({
    selector: 'app-root',
    imports: [RouterOutlet],
    templateUrl: './app.html',
    styleUrl: './app.scss'
})
export class App implements OnInit {
    private store = inject(EnrollmentStore);

    protected readonly title = signal('tms-client');

    ngOnInit() {
        this.store.loadEnrollments();
        this.store.listenForLiveUpdates();
    }
}