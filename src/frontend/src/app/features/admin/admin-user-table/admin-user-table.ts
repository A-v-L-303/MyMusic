import { Component, input, output } from '@angular/core';
import { LucideTrash2 } from '@lucide/angular';

import { Pagination } from '../../../shared/pagination/pagination';
import { AdminUser } from '../admin-user';

@Component({
  selector: 'app-admin-user-table',
  imports: [Pagination, LucideTrash2],
  templateUrl: './admin-user-table.html',
})
export class AdminUserTable {
  readonly users = input.required<AdminUser[]>();
  readonly loading = input.required<boolean>();
  readonly page = input.required<number>();
  readonly totalPages = input.required<number>();
  readonly currentUserId = input<string | undefined>();

  readonly deleteRequested = output<AdminUser>();
  readonly pageChange = output<number>();
}
