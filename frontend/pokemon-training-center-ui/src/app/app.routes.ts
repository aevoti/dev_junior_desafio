import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'matriculas', pathMatch: 'full' },
  {
    path: 'matriculas',
    loadComponent: () => import('./features/enrollments/list/enrollment-list.component')
      .then(m => m.EnrollmentListComponent)
  },
  {
    path: 'matriculas/nova',
    loadComponent: () => import('./features/enrollments/form/enrollment-form.component')
      .then(m => m.EnrollmentFormComponent)
  },
  {
  path: 'pokemons/transferir',
  loadComponent: () => import('./features/pokemons/transfer/pokemon-transfer.component')
    .then(m => m.PokemonTransferComponent)
}
];