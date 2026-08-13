import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';

@Component({
  selector: 'app-search',
  templateUrl: './search.html',
})
export class Search {
  private readonly route = inject(ActivatedRoute);

  protected readonly query = toSignal(
    this.route.queryParamMap.pipe(map((params) => params.get('q'))),
    { initialValue: null },
  );
}
