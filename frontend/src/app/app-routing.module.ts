import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ListaMatriculasComponent } from './components/matriculas/lista-matriculas/lista-matriculas.component';
import { NovaMatriculaComponent } from './components/matriculas/nova-matricula/nova-matricula.component';

const routes: Routes = [
  { path: '', redirectTo: '/matriculas', pathMatch: 'full' },
  { path: 'matriculas', component: ListaMatriculasComponent },
  { path: 'matriculas/nova', component: NovaMatriculaComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
