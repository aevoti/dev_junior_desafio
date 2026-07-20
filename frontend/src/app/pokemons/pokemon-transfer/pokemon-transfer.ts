import { Component, OnInit, inject } from '@angular/core';
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

  pokemons: Pokemon[] = [];
  trainers: Trainer[] = [];
  errorMessage: string | null = null;
  result: TransferPokemonResponse | null = null;
  submitting = false;

  readonly form = this.fb.nonNullable.group({
    pokemonId: [null as number | null, Validators.required],
    newTrainerId: [null as number | null, Validators.required],
  });

  ngOnInit(): void {
    this.pokemonApi.list().subscribe((pokemons) => (this.pokemons = pokemons));
    this.trainerApi.list().subscribe((trainers) => (this.trainers = trainers));
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage = null;
    this.result = null;
    this.submitting = true;

    const value = this.form.getRawValue();
    this.pokemonApi.transfer(value.pokemonId!, { newTrainerId: value.newTrainerId! }).subscribe({
      next: (result) => {
        this.submitting = false;
        this.result = result;
        this.form.reset();
      },
      // R5: destino inexistente ou igual ao Treinador atual chega aqui como mensagem amigável.
      error: (err: Error) => {
        this.submitting = false;
        this.errorMessage = err.message;
      },
    });
  }
}
