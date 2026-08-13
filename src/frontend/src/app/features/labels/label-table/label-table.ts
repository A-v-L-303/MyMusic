import { Component, input, output } from '@angular/core';
import { LucidePencil, LucideTrash2 } from '@lucide/angular';

import { Pagination } from '../../../shared/pagination/pagination';
import { Label } from '../label';

@Component({
  selector: 'app-label-table',
  imports: [Pagination, LucidePencil, LucideTrash2],
  templateUrl: './label-table.html',
})
export class LabelTable {
  readonly labels = input.required<Label[]>();
  readonly loading = input.required<boolean>();
  readonly page = input.required<number>();
  readonly totalPages = input.required<number>();

  readonly editRequested = output<Label>();
  readonly deleteRequested = output<Label>();
  readonly pageChange = output<number>();
}
