import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'matriculas', pathMatch: 'full' },
  {
    path: 'matriculas',
    loadComponent: () => import('./enrollments/enrollments-list/enrollments-list').then((m) => m.EnrollmentsList),
  },
  {
    path: 'matriculas/nova',
    loadComponent: () => import('./enrollments/enrollment-form/enrollment-form').then((m) => m.EnrollmentForm),
  },
  {
    path: 'treinadores',
    loadComponent: () => import('./trainers/trainers-list/trainers-list').then((m) => m.TrainersList),
  },
  {
    path: 'treinadores/novo',
    loadComponent: () => import('./trainers/trainer-form/trainer-form').then((m) => m.TrainerForm),
  },
  {
    path: 'pokemons',
    loadComponent: () => import('./pokemons/pokemons-list/pokemons-list').then((m) => m.PokemonsList),
  },
  {
    path: 'pokemons/novo',
    loadComponent: () => import('./pokemons/pokemon-form/pokemon-form').then((m) => m.PokemonForm),
  },
  {
    path: 'matriculas/:id/upgrade',
    loadComponent: () =>
      import('./enrollments/enrollment-upgrade/enrollment-upgrade').then((m) => m.EnrollmentUpgrade),
  },
  {
    path: 'pokemons/transferir',
    loadComponent: () =>
      import('./pokemons/pokemon-transfer/pokemon-transfer').then((m) => m.PokemonTransfer),
  },
];
