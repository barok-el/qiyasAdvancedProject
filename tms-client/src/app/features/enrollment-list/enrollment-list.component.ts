import { Component, effect, inject, input, viewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';

import { Enrollment } from '../../models/enrollment.model';
import { EnrollmentStore } from '../../store/enrollment.store';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'tms-enrollment-list',
  standalone: true,
  imports: [
    MatTableModule,
    MatPaginatorModule,
    MatSortModule
  ],
  templateUrl: './enrollment-list.component.html',
  styleUrl: './enrollment-list.component.scss'
})
export class EnrollmentListComponent {
  store = inject(EnrollmentStore);
  private readonly auth = inject(AuthService);
  readonly management = input(true);
  readonly title = input('Enrollment Records');

  get displayedColumns(): string[] {
    return this.management()
      ? ['studentName', 'courseName', 'status', 'actions']
      : ['courseName', 'status'];
  }

  dataSource = new MatTableDataSource<Enrollment>();

  readonly paginator = viewChild.required(MatPaginator);
  readonly sort = viewChild.required(MatSort);

  constructor() {
    effect(() => {
      this.dataSource.data = this.store.entities();
    });

    effect(() => {
      this.dataSource.paginator = this.paginator();
      this.dataSource.sort = this.sort();
    });
  }

  ngOnInit(): void {
    if (this.management()) {
      this.store.loadEnrollments(this.auth.hasRole('Admin') ? 'admin' : 'instructor');
    }
  }
}
