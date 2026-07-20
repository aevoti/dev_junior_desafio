import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

import { Matricula, UpgradeSimulacao } from '../../models/matricula.model';
import { Plano } from '../../models/plano.model';
import { Pokemon } from '../../models/pokemon.model';
import { MatriculaService } from '../../services/matricula.service';
import { PlanoService } from '../../services/plano.service';
import { PokemonService } from '../../services/pokemon.service';

const NOME_PLANO_ELITE = 'Elite dos 4';
const NIVEL_MINIMO_ELITE = 50;

@Component({
  selector: 'app-formulario-upgrade-matricula',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    CurrencyPipe,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './formulario-upgrade-matricula.html',
  styleUrl: './formulario-upgrade-matricula.css',
})
export class FormularioUpgradeMatricula {
  private readonly route = inject(ActivatedRoute);
  private readonly matriculaService = inject(MatriculaService);
  private readonly planoService = inject(PlanoService);
  private readonly pokemonService = inject(PokemonService);

  private readonly matriculaId = Number(this.route.snapshot.paramMap.get('id'));

  matricula = signal<Matricula | null>(null);
  pokemon = signal<Pokemon | null>(null);
  planos = signal<Plano[]>([]);

  carregando = signal(true);
  erro = signal<string | null>(null);
  sucesso = signal(false);

  simulando = signal(false);
  confirmando = signal(false);
  simulacao = signal<UpgradeSimulacao | null>(null);

  novoPlanoId = new FormControl<number | null>(null, Validators.required);
  private readonly novoPlanoIdValor = toSignal(this.novoPlanoId.valueChanges, {
    initialValue: null as number | null,
  });

  planosDisponiveis = computed(() => {
    const atual = this.matricula();
    if (!atual) {
      return [];
    }
    return this.planos().filter((plano) => plano.valorMensal > atual.planoValorMensal);
  });

  planoSelecionado = computed(
    () => this.planos().find((plano) => plano.id === this.novoPlanoIdValor()) ?? null,
  );

  bloqueadoPorNivel = computed(() => {
    const plano = this.planoSelecionado();
    const pokemon = this.pokemon();
    return !!plano && plano.nome === NOME_PLANO_ELITE && !!pokemon && pokemon.nivel < NIVEL_MINIMO_ELITE;
  });

  constructor() {
    this.matriculaService.obterPorId(this.matriculaId).subscribe({
      next: (matricula) => {
        this.carregando.set(false);
        this.matricula.set(matricula);
        this.pokemonService.obterPorId(matricula.pokemonId).subscribe({
          next: (pokemon) => this.pokemon.set(pokemon),
        });
      },
      error: (erro: HttpErrorResponse) => {
        this.carregando.set(false);
        this.erro.set(erro.error?.mensagem ?? 'Matrícula não encontrada.');
      },
    });

    this.planoService.listar().subscribe({
      next: (planos) => this.planos.set(planos),
    });

    this.novoPlanoId.valueChanges.subscribe(() => this.simulacao.set(null));
  }

  simular(): void {
    if (this.novoPlanoId.invalid || this.bloqueadoPorNivel()) {
      this.novoPlanoId.markAsTouched();
      return;
    }

    this.simulando.set(true);
    this.erro.set(null);

    this.matriculaService.simularUpgrade(this.matriculaId, this.novoPlanoId.value!).subscribe({
      next: (resultado) => {
        this.simulando.set(false);
        this.simulacao.set(resultado);
      },
      error: (erro: HttpErrorResponse) => {
        this.simulando.set(false);
        this.erro.set(erro.error?.mensagem ?? 'Ocorreu um erro inesperado. Tente novamente.');
      },
    });
  }

  confirmar(): void {
    if (!this.simulacao() || !this.novoPlanoId.value) {
      return;
    }

    this.confirmando.set(true);
    this.erro.set(null);

    this.matriculaService
      .confirmarUpgrade(this.matriculaId, { novoPlanoId: this.novoPlanoId.value })
      .subscribe({
        next: (atualizada) => {
          this.confirmando.set(false);
          this.sucesso.set(true);
          this.matricula.set(atualizada);
        },
        error: (erro: HttpErrorResponse) => {
          this.confirmando.set(false);
          this.erro.set(erro.error?.mensagem ?? 'Ocorreu um erro inesperado. Tente novamente.');
        },
      });
  }
}
