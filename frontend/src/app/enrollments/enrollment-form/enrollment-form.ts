import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { Pokemon } from '../../shared/models/pokemon.model';
import { TrainingPlan } from '../../shared/models/training-plan.model';
import { EnrollmentApiService } from '../../shared/services/enrollment-api.service';
import { PokemonApiService } from '../../shared/services/pokemon-api.service';
import { TrainingPlanApiService } from '../../shared/services/training-plan-api.service';

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
  private readonly router = inject(Router);

  // signal(), não campos soltos (change detection zoneless)
  readonly pokemons = signal<Pokemon[]>([]);
  readonly trainingPlans = signal<TrainingPlan[]>([]);
  readonly errorMessage = signal<string | null>(null);
  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    pokemonId: [null as number | null, Validators.required],
    trainingPlanId: [null as number | null, Validators.required],
  });

  private readonly selectedPokemonId = signal<number | null>(null);
  private readonly selectedTrainingPlanId = signal<number | null>(null);

  // computed() é um signal cujo valor é derivado de outros signals
  // aqui, visa validar se o nível do pokémon é suficiente para o plano
  // (se for elite dos 4) sem precisar submeter ao backend. recalcula
  // sempre os signals de que depende mudarem
  readonly eliteDosQuatroWarning = computed(() => {
    const pokemon = this.pokemons().find((p) => p.id === this.selectedPokemonId());
    const plan = this.trainingPlans().find((p) => p.id === this.selectedTrainingPlanId());

    if (!pokemon || !plan || plan.name !== ELITE_DOS_QUATRO_PLAN_NAME) {
      return null;
    }

    return pokemon.level < ELITE_DOS_QUATRO_MINIMUM_LEVEL
      ? `Nível mínimo de ${ELITE_DOS_QUATRO_MINIMUM_LEVEL} é necessário para o plano Elite dos 4 (este Pokémon está no nível ${pokemon.level}).`
      : null;
  });

  ngOnInit(): void {
    this.pokemonApi.list().subscribe((pokemons) => this.pokemons.set(pokemons));
    this.trainingPlanApi.list().subscribe((plans) => this.trainingPlans.set(plans));

    // o FormControl não é um signal (não avisa das suas mudanças ao Angular)
    // como precisamos reagir às mudanças desses campos e estamos trabalhando
    // com Angular moderno (signals), fazemos uma adaptação: criamos um signal
    // para cada FormControl e o atualizamos sempre que valueChanges
    this.form.controls.pokemonId.valueChanges.subscribe((id) => this.selectedPokemonId.set(id));
    this.form.controls.trainingPlanId.valueChanges.subscribe((id) => this.selectedTrainingPlanId.set(id));
  }

  submit(): void {
    if (this.form.invalid || this.eliteDosQuatroWarning()) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.submitting.set(true);

    const value = this.form.getRawValue();
    this.enrollmentApi
      .create({ pokemonId: value.pokemonId!, trainingPlanId: value.trainingPlanId! })
      .subscribe({
        // FR-023: confirmação de sucesso + redirecionamento para a listagem.
        next: () => {
          this.submitting.set(false);
          this.router.navigate(['/matriculas'], { state: { successMessage: 'Matrícula criada com sucesso.' } });
        },
        // R1 (matrícula duplicada) e R3 (nível mínimo) chegam aqui como mensagem amigável já em português.
        error: (err: Error) => {
          this.submitting.set(false);
          this.errorMessage.set(err.message);
        },
      });
  }
}
