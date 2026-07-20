import { Routes } from '@angular/router';
import { Dashboard } from './pages/dashboard/dashboard';
import { Matriculas } from './pages/matriculas/matriculas';
import { Planos } from './pages/planos/planos';
import { Pokemons } from './pages/pokemons/pokemons';
import { Treinadores } from './pages/treinadores/treinadores';

export const routes: Routes = [
  {
    path: '',
    component: Dashboard
  },
  {
    path: 'treinadores',
    component: Treinadores
  },
  {
    path: 'pokemons',
    component: Pokemons
  },
  {
    path: 'planos',
    component: Planos
  },
  {
    path: 'matriculas',
    component: Matriculas
  },
  {
    path: '**',
    redirectTo: ''
  }
];