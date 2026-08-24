import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';

import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { DashboardService } from './dashboard.service';
import { FormatChart } from './format-chart/format-chart';
import { StatTile } from './stat-tile/stat-tile';
import { TopArtists } from './top-artists/top-artists';
import { TopLabels } from './top-labels/top-labels';
import { YearDistribution } from './year-distribution/year-distribution';

@Component({
  selector: 'app-dashboard',
  imports: [StatTile, FormatChart, TopArtists, TopLabels, YearDistribution],
  templateUrl: './dashboard.html',
})
export class Dashboard {
  private readonly dashboardService = inject(DashboardService);
  private readonly errorModalService = inject(ErrorModalService);

  protected readonly dashboardResource = rxResource({
    stream: () => this.dashboardService.getDashboard(),
  });

  constructor() {
    effect(() => {
      const error = this.dashboardResource.error();

      if (error instanceof HttpErrorResponse) {
        this.errorModalService.showFromHttpError(error, 'Dashboard', () =>
          this.dashboardResource.reload(),
        );
      }
    });
  }
}
