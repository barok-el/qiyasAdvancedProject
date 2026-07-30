import { Component, inject } from '@angular/core';
import { EnrollmentStore } from '../../store/enrollment.store';
@Component({
  selector: 'tms-enrollment-list',
  standalone: true,
  templateUrl: './enrollment-list.component.html'
})
export class EnrollmentListComponent {
  store = inject(EnrollmentStore);

  onApprove(id: number) {
    this.store.approveEnrollment(id);
  }
}
