import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { POKEMON_TYPES } from '../../shared/models/pokemon.model';
import { Trainer } from '../../shared/models/trainer.model';
import { PokemonApiService } from '../../shared/services/pokemon-api.service';
import { TrainerApiService } from '../../shared/services/trainer-api.service';

@Component({
  selector: 'app-pokemon-form',
  imports: [ReactiveFormsModule],
  templateUrl: './pokemon-form.html',
  styleUrl: './pokemon-form.css',
})
export class PokemonForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly pokemonApi = inject(PokemonApiService);
  private readonly trainerApi = inject(TrainerApiService);
  private readonly router = inject(Router);

  readonly pokemonTypes = POKEMON_TYPES;

  // signal(), não campos soltos — ver enrollments-list.ts/app.config.ts.
  readonly trainers = signal<Trainer[]>([]);
  readonly errorMessage = signal<string | null>(null);
  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    type: ['', Validators.required],
    // FR-004: nível entre 1 e 100.
    level: [1, [Validators.required, Validators.min(1), Validators.max(100)]],
    trainerId: [null as number | null, Validators.required],
  });

  ngOnInit(): void {
    this.trainerApi.list().subscribe((trainers) => this.trainers.set(trainers));
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.submitting.set(true);

    const value = this.form.getRawValue();
    this.pokemonApi
      .create({ name: value.name, type: value.type, level: value.level, trainerId: value.trainerId! })
      .subscribe({
        // FR-023: confirmação de sucesso + redirecionamento para a listagem.
        next: () => {
          this.submitting.set(false);
          this.router.navigate(['/pokemons'], { state: { successMessage: 'Pokémon cadastrado com sucesso.' } });
        },
        error: (err: Error) => {
          this.submitting.set(false);
          this.errorMessage.set(err.message);
        },
      });
  }
}
