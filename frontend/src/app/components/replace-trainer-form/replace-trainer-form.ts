import { Component, OnInit, EventEmitter, Output, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-replace-trainer-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './replace-trainer-form.html',
  styleUrl: './replace-trainer-form.css'
})
export class ReplaceTrainerFormComponent implements OnInit {

  pokemonList: any[] = [];
  trainerList: any[] = [];

  selectedPokemonId: number | null = null;
  selectedTrainerId: number | null = null;

  message: string = '';
  isError: boolean = false;

  @Output() replaceComplete = new EventEmitter<void>();

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadPokemons();
    this.loadTrainers();
  }

  loadPokemons() {
    this.http.get<any[]>('http://localhost:5141/api/pokemon').subscribe({
      next: (data) => {
        this.pokemonList = data;
        this.cdr.detectChanges();
      },
      error: () => console.error('Erro ao carregar pokémons')
    });
  }

  loadTrainers() {
    this.http.get<any[]>('http://localhost:5141/api/trainer').subscribe({
      next: (data) => {
        this.trainerList = data;
        this.cdr.detectChanges();
      },
      error: () => console.error('Erro ao carregar treinadores')
    });
  }

  onSubmit() {
    if (!this.selectedPokemonId || !this.selectedTrainerId) {
      this.isError = true;
      this.message = 'Preencha todos os campos';
      this.cdr.detectChanges();
      return;
    }

    const url = `http://localhost:5141/api/pokemon/${this.selectedPokemonId}/replace-trainer`;

    this.http.patch(url, this.selectedTrainerId, {
      headers: { 'Content-Type': 'application/json' }
    }).subscribe({
      next: (response: any) => {
        this.isError = false;
        this.message = response.message;
        this.selectedPokemonId = null;
        this.selectedTrainerId = null;
        this.cdr.detectChanges();
        this.replaceComplete.emit();
      },
      error: (err) => {
        this.isError = true;
        this.message = err.error?.message || 'Erro ao replaceir Pokémon.';
        this.cdr.detectChanges();
      }
    });
  }
}