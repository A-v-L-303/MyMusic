import { Routes } from '@angular/router';

import { RecordDetail } from './record-detail/record-detail';
import { Records } from './records';

export const recordsRoutes: Routes = [
  {
    path: '',
    component: Records,
    children: [{ path: ':id', component: RecordDetail }],
  },
];
