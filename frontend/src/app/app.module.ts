import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ListaMatriculasComponent } from './components/matriculas/lista-matriculas/lista-matriculas.component';
import { NovaMatriculaComponent } from './components/matriculas/nova-matricula/nova-matricula.component';
import { UpgradeMatriculaComponent } from './components/matriculas/upgrade-matricula/upgrade-matricula.component';

@NgModule({
  declarations: [
    AppComponent,
    ListaMatriculasComponent,
    NovaMatriculaComponent,
    UpgradeMatriculaComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    ReactiveFormsModule,
    FormsModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
