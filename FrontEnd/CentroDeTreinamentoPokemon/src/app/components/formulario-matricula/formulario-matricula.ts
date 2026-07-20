import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

import { NovaMatriculaPayload } from '../../models/matricula.model';
import { Pokemon } from '../../models/pokemon.model';
import { Plano } from '../../models/plano.model';
import { Treinador } from '../../models/treinador.model';
import { MatriculaService } from '../../services/matricula.service';
import { PlanoService } from '../../services/plano.service';
import { PokemonService } from '../../services/pokemon.service';
import { TreinadorService } from '../../services/treinador.service';

const NOME_PLANO_ELITE = 'Elite dos 4';
const NIVEL_MINIMO_ELITE = 50;

@Component({
  selector: 'app-formulario-matricula',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatSelectModule, MatButtonModule],
  templateUrl: './formulario-matricula.html',
  styleUrl: './formulario-matricula.css',
})
export class FormularioMatricula {
  private readonly matriculaService = inject(MatriculaService);
  private readonly pokemonService = inject(PokemonService);
  private readonly planoService = inject(PlanoService);
  private readonly treinadorSerivice = inject(TreinadorService);

  treinadores = signal<Treinador[]>([]);
  pokemons = signal<Pokemon[]>([]);
  planos = signal<Plano[]>([]);
  pokemonIdsComMatriculaAtiva = signal<Set<number>>(new Set());

  form = new FormGroup(
    {
      treinadorId: new FormControl<number | null>(null, Validators.required),
      pokemonId: new FormControl<number | null>(null, Validators.required),
      planoId: new FormControl<number | null>(null, Validators.required),
    },
    { validators: [this.nivelMinimoEliteValidator()] },
  );

  private readonly treinadorIdValor = toSignal(this.form.controls.treinadorId.valueChanges, {
    initialValue: null as number | null,
  });

  pokemonsDisponiveis = computed(() => {
    const treinadorId = this.treinadorIdValor();
    if (!treinadorId) {
      return [];
    }
    return this.pokemons().filter(
      (pokemon) =>
        pokemon.treinadorId === treinadorId && !this.pokemonIdsComMatriculaAtiva().has(pokemon.id),
    );
  });

  carregando = signal(false);
  erro = signal<string | null>(null);
  sucesso = signal(false);

  constructor() {
    this.treinadorSerivice.listar().subscribe({
      next: (treinadores) => this.treinadores.set(treinadores),
    });

    this.pokemonService.listar().subscribe({
      next: (pokemons) => this.pokemons.set(pokemons),
    });

    this.planoService.listar().subscribe({
      next: (planos) => this.planos.set(planos),
    });

    this.matriculaService.listar().subscribe({
      next: (matriculas) => {
        const ids = matriculas
          .filter((matricula) => matricula.status === 'Ativa')
          .map((matricula) => matricula.pokemonId);
        this.pokemonIdsComMatriculaAtiva.set(new Set(ids));
      },
    });

    this.form.controls.treinadorId.valueChanges.subscribe(() => {
      this.form.controls.pokemonId.setValue(null);
    });
  }

  private nivelMinimoEliteValidator(): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const pokemonId = group.get('pokemonId')?.value;
      const planoId = group.get('planoId')?.value;
      if (!pokemonId || !planoId) {
        return null;
      }

      const pokemon = this.pokemons().find((item) => item.id === pokemonId);
      const plano = this.planos().find((item) => item.id === planoId);
      if (!pokemon || !plano) {
        return null;
      }

      if (plano.nome === NOME_PLANO_ELITE && pokemon.nivel < NIVEL_MINIMO_ELITE) {
        return { nivelMinimoElite: true };
      }

      return null;
    };
  }

  enviar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando.set(true);
    this.erro.set(null);
    this.sucesso.set(false);

    const valores = this.form.getRawValue();
    const payload: NovaMatriculaPayload = {
      pokemonId: valores.pokemonId!,
      planoId: valores.planoId!,
    };

    this.matriculaService.criar(payload).subscribe({
      next: () => {
        this.carregando.set(false);
        this.sucesso.set(true);
        this.pokemonIdsComMatriculaAtiva.update((ids) => new Set(ids).add(payload.pokemonId));
        this.form.reset();
      },
      error: (erro: HttpErrorResponse) => {
        this.carregando.set(false);
        this.erro.set(erro.error?.mensagem ?? 'Ocorreu um erro inesperado. Tente novamente.');
      },
    });
  }
}
