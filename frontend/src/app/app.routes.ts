import { Routes } from '@angular/router';
import { MatriculasPage } from './features/matriculas/pages/matriculas-page/matriculas-page';

export const routes: Routes = [
  { path: '', component: MatriculasPage },
  { path: '**', redirectTo: '' }
];
