import { Component } from '@angular/core';
import { TrainerFormComponent } from './components/trainer-form/trainer-form'; 
import { PokemonFormComponent } from './components/pokemon-form/pokemon-form';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent {
  title = 'centro-pokemon-ui';
}