import { Component, OnInit, inject, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { POKEMON_TYPES, Pokemon } from '../../shared/models/pokemon.model';
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

  readonly created = output<Pokemon>();
  readonly pokemonTypes = POKEMON_TYPES;

  trainers: Trainer[] = [];
  errorMessage: string | null = null;
  submitting = false;

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    type: ['', Validators.required],
    // FR-004: nível entre 1 e 100.
    level: [1, [Validators.required, Validators.min(1), Validators.max(100)]],
    trainerId: [null as number | null, Validators.required],
  });

  ngOnInit(): void {
    this.trainerApi.list().subscribe((trainers) => (this.trainers = trainers));
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage = null;
    this.submitting = true;

    const value = this.form.getRawValue();
    this.pokemonApi
      .create({ name: value.name, type: value.type, level: value.level, trainerId: value.trainerId! })
      .subscribe({
        next: (pokemon) => {
          this.submitting = false;
          this.form.reset({ level: 1 });
          this.created.emit(pokemon);
        },
        error: (err: Error) => {
          this.submitting = false;
          this.errorMessage = err.message;
        },
      });
  }
}
