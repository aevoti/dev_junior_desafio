import { Routes } from '@angular/router';

import { FormularioMatricula } from './components/formulario-matricula/formulario-matricula';
import { FormularioPokemon } from './components/formulario-pokemon/formulario-pokemon';
import { FormularioTreinador } from './components/formulario-treinador/formulario-treinador';
import { FormularioUpgradeMatricula } from './components/formulario-upgrade-matricula/formulario-upgrade-matricula';
import { ListaDeMatriculas } from './components/lista-de-matriculas/lista-de-matriculas';

export const routes: Routes = [
  { path: '', component: ListaDeMatriculas },
  { path: 'matriculas/nova', component: FormularioMatricula },
  { path: 'matriculas/:id/upgrade', component: FormularioUpgradeMatricula },
  { path: 'treinadores/novo', component: FormularioTreinador },
  { path: 'pokemons/novo', component: FormularioPokemon },
];
