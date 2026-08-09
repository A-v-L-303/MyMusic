import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';
import { HomePlaceholder } from './core/shell/home-placeholder/home-placeholder';

export const routes: Routes = [
  { path: '', component: HomePlaceholder, canActivate: [authGuard] },
  { path: '**', component: HomePlaceholder, canActivate: [authGuard] },
];
