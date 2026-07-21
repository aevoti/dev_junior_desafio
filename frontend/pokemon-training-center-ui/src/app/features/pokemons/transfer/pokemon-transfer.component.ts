import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PokemonService } from '../../../core/services/pokemon.service';
import { TrainerService } from '../../../core/services/trainer.service';
import { Pokemon } from '../../../shared/models/pokemon.model';
import { Trainer } from '../../../shared/models/trainer.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ParallaxHeroComponent } from '../../../shared/components/parallax-hero/parallax-hero.component';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-pokemon-transfer',
  standalone: true,
  imports: [CommonModule, FormsModule, ParallaxHeroComponent, RouterLink],
  templateUrl: './pokemon-transfer.component.html',
  styleUrl: './pokemon-transfer.component.scss'
})
export class PokemonTransferComponent implements OnInit {
  pokemons = signal<Pokemon[]>([]);
  trainers = signal<Trainer[]>([]);
  loading = signal(true);
  transferindo = signal(false);

  pokemonSelecionadoId: number | null = null;
  novoTreinadorId: number | null = null;

  constructor(
    private pokemonService: PokemonService,
    private trainerService: TrainerService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.loading.set(true);
    this.pokemonService.getAll().subscribe({
      next: pokemons => {
        this.pokemons.set(pokemons);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    this.trainerService.getAll().subscribe(trainers => this.trainers.set(trainers));
  }

  get pokemonSelecionado(): Pokemon | undefined {
    return this.pokemons().find(p => p.id === this.pokemonSelecionadoId);
  }

  get treinadoresDisponiveis(): Trainer[] {
    const donoAtualId = this.pokemonSelecionado?.treinadorId;
    return this.trainers().filter(t => t.id !== donoAtualId);
  }

  transferir(): void {
    if (!this.pokemonSelecionadoId || !this.novoTreinadorId) return;

    this.transferindo.set(true);
    this.pokemonService.transferir(this.pokemonSelecionadoId, this.novoTreinadorId).subscribe({
      next: () => {
        this.toastService.show('Pokémon transferido com sucesso!', 'success');
        this.novoTreinadorId = null;
        this.pokemonSelecionadoId = null;
        this.transferindo.set(false);
        this.carregar();
      },
      error: () => this.transferindo.set(false)
    });
  }
}