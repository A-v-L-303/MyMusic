import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadChildren: () => import('./features/dashboard/dashboard.routes').then((m) => m.dashboardRoutes),
      },
      {
        path: 'records',
        loadChildren: () => import('./features/records/records.routes').then((m) => m.recordsRoutes),
      },
      {
        path: 'artists',
        loadChildren: () => import('./features/artists/artists.routes').then((m) => m.artistsRoutes),
      },
      {
        path: 'labels',
        loadChildren: () => import('./features/labels/labels.routes').then((m) => m.labelsRoutes),
      },
      {
        path: 'genres',
        loadChildren: () => import('./features/genres/genres.routes').then((m) => m.genresRoutes),
      },
      {
        path: 'search',
        loadChildren: () => import('./features/search/search.routes').then((m) => m.searchRoutes),
      },
      { path: '**', redirectTo: 'dashboard' },
    ],
  },
];
