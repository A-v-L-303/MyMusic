import { Component, input } from '@angular/core';

@Component({
  selector: 'app-stat-tile',
  host: { class: 'contents' },
  templateUrl: './stat-tile.html',
})
export class StatTile {
  readonly label = input.required<string>();
  readonly value = input.required<number>();
}
