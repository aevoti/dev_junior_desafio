import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-pokemon-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pokemon-form.html',
  styleUrl: './pokemon-form.css'
})
export class PokemonFormComponent implements OnInit {
  pokemon = {
    Name: '',
    Type: '',
    Level: '',
    TrainerId: ''
  };

  trainersList: any[] = [];

  message: string = '';
  isError: boolean = false;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadTrainers();
  }

  loadTrainers() {
    this.http.get<any[]>('http://localhost:5141/api/trainer').subscribe({
      next: (data) => {
        this.trainersList = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erro ao carregar treinadores:', err)
      }
    })
  }

  onSubmit() {
    if (!this.pokemon.Name.trim() || !this.pokemon.Type.trim() || !this.pokemon.Level || ! this.pokemon.TrainerId) {
      this.isError = true;
      this.message = 'Preencha todos os campos';
      this.cdr.detectChanges();
      return;
    }

    const apiUrl = 'http://localhost:5141/api/pokemon';

    this.http.post(apiUrl, this.pokemon).subscribe({
      next: (response: any) => {
        this.message = response.message;
        this.isError = false;
        this.pokemon = { Name: '', Type: '', Level: '', TrainerId: '' };
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isError = true;
        this.message = err.error?.message || 'Erro ao conectar com o servidor.';
        this.cdr.detectChanges();
      }
    });
  }
}