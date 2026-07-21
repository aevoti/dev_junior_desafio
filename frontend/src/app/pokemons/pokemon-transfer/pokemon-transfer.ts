import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { Pokemon, TransferPokemonResponse } from '../../shared/models/pokemon.model';
import { Trainer } from '../../shared/models/trainer.model';
import { PokemonApiService } from '../../shared/services/pokemon-api.service';
import { TrainerApiService } from '../../shared/services/trainer-api.service';

@Component({
  selector: 'app-pokemon-transfer',
  imports: [ReactiveFormsModule],
  templateUrl: './pokemon-transfer.html',
  styleUrl: './pokemon-transfer.css',
})
export class PokemonTransfer implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly pokemonApi = inject(PokemonApiService);
  private readonly trainerApi = inject(TrainerApiService);

  // signal(), não campos soltos — ver enrollments-list.ts/app.config.ts.
  readonly pokemons = signal<Pokemon[]>([]);
  readonly trainers = signal<Trainer[]>([]);
  readonly errorMessage = signal<string | null>(null);
  readonly result = signal<TransferPokemonResponse | null>(null);
  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    pokemonId: [null as number | null, Validators.required],
    newTrainerId: [null as number | null, Validators.required],
  });

  ngOnInit(): void {
    this.pokemonApi.list().subscribe((pokemons) => this.pokemons.set(pokemons));
    this.trainerApi.list().subscribe((trainers) => this.trainers.set(trainers));
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.result.set(null);
    this.submitting.set(true);

    const value = this.form.getRawValue();
    this.pokemonApi.transfer(value.pokemonId!, { newTrainerId: value.newTrainerId! }).subscribe({
      next: (result) => {
        this.submitting.set(false);
        this.result.set(result);
        this.form.reset();
      },
      // R5: destino inexistente ou igual ao Treinador atual chega aqui como mensagem amigável.
      error: (err: Error) => {
        this.submitting.set(false);
        this.errorMessage.set(err.message);
      },
    });
  }
}
