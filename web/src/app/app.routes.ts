import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'matriculas', pathMatch: 'full' },
  {
    path: 'matriculas',
    loadComponent: () =>
      import('./features/matriculas/pages/matriculas-list/matriculas-list.component').then(
        (m) => m.MatriculasListComponent,
      ),
  },
  {
    path: 'matriculas/nova',
    loadComponent: () =>
      import('./features/matriculas/pages/matricula-form/matricula-form.component').then(
        (m) => m.MatriculaFormComponent,
      ),
  },
  { path: '**', redirectTo: 'matriculas' },
];
