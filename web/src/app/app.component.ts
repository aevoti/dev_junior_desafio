import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <header>
      <h1>Centro de Treinamento Pokémon - Alto Nível</h1>
      <nav>
        <a routerLink="/matriculas">Matrículas</a>
        <a routerLink="/matriculas/nova">Nova Matrícula</a>
      </nav>
    </header>
    <main>
      <router-outlet />
    </main>
  `,
})
export class AppComponent {}
