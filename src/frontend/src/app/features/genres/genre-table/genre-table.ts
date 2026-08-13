import { Component, input, output } from '@angular/core';
import { LucidePencil, LucideTrash2 } from '@lucide/angular';

import { Pagination } from '../../../shared/pagination/pagination';
import { Genre } from '../genre';

@Component({
  selector: 'app-genre-table',
  imports: [Pagination, LucidePencil, LucideTrash2],
  templateUrl: './genre-table.html',
})
export class GenreTable {
  readonly genres = input.required<Genre[]>();
  readonly loading = input.required<boolean>();
  readonly page = input.required<number>();
  readonly totalPages = input.required<number>();

  readonly editRequested = output<Genre>();
  readonly deleteRequested = output<Genre>();
  readonly pageChange = output<number>();
}
