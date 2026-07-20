import { Component, OnInit, computed, inject, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { Enrollment } from '../shared/models/enrollment.model';
import { Pokemon } from '../shared/models/pokemon.model';
import { TrainingPlan } from '../shared/models/training-plan.model';
import { EnrollmentApiService } from '../shared/services/enrollment-api.service';
import { PokemonApiService } from '../shared/services/pokemon-api.service';
import { TrainingPlanApiService } from '../shared/services/training-plan-api.service';

const ELITE_DOS_QUATRO_PLAN_NAME = 'Elite dos 4';
const ELITE_DOS_QUATRO_MINIMUM_LEVEL = 50;

@Component({
  selector: 'app-enrollment-form',
  imports: [ReactiveFormsModule],
  templateUrl: './enrollment-form.html',
  styleUrl: './enrollment-form.css',
})
export class EnrollmentForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly enrollmentApi = inject(EnrollmentApiService);
  private readonly pokemonApi = inject(PokemonApiService);
  private readonly trainingPlanApi = inject(TrainingPlanApiService);

  readonly created = output<Enrollment>();

  pokemons: Pokemon[] = [];
  trainingPlans: TrainingPlan[] = [];
  errorMessage: string | null = null;
  submitting = false;

  readonly form = this.fb.nonNullable.group({
    pokemonId: [null as number | null, Validators.required],
    trainingPlanId: [null as number | null, Validators.required],
  });

  private readonly selectedPokemonId = signal<number | null>(null);
  private readonly selectedTrainingPlanId = signal<number | null>(null);

  /** Validação de R3 (FR-018) no cliente, antes mesmo de submeter ao backend. */
  readonly eliteDosQuatroWarning = computed(() => {
    const pokemon = this.pokemons.find((p) => p.id === this.selectedPokemonId());
    const plan = this.trainingPlans.find((p) => p.id === this.selectedTrainingPlanId());

    if (!pokemon || !plan || plan.name !== ELITE_DOS_QUATRO_PLAN_NAME) {
      return null;
    }

    return pokemon.level < ELITE_DOS_QUATRO_MINIMUM_LEVEL
      ? `Nível mínimo de ${ELITE_DOS_QUATRO_MINIMUM_LEVEL} é necessário para o plano Elite dos 4 (este Pokémon está no nível ${pokemon.level}).`
      : null;
  });

  ngOnInit(): void {
    this.pokemonApi.list().subscribe((pokemons) => (this.pokemons = pokemons));
    this.trainingPlanApi.list().subscribe((plans) => (this.trainingPlans = plans));

    this.form.controls.pokemonId.valueChanges.subscribe((id) => this.selectedPokemonId.set(id));
    this.form.controls.trainingPlanId.valueChanges.subscribe((id) => this.selectedTrainingPlanId.set(id));
  }

  submit(): void {
    if (this.form.invalid || this.eliteDosQuatroWarning()) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage = null;
    this.submitting = true;

    const value = this.form.getRawValue();
    this.enrollmentApi
      .create({ pokemonId: value.pokemonId!, trainingPlanId: value.trainingPlanId! })
      .subscribe({
        next: (enrollment) => {
          this.submitting = false;
          this.form.reset();
          this.created.emit(enrollment);
        },
        // R1 (matrícula duplicada) e R3 (nível mínimo) chegam aqui como mensagem amigável já em português.
        error: (err: Error) => {
          this.submitting = false;
          this.errorMessage = err.message;
        },
      });
  }
}
