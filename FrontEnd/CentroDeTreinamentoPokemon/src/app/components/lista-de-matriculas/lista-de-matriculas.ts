import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';

import { Matricula } from '../../models/matricula.model';
import { MatriculaService } from '../../services/matricula.service';

type FiltroStatus = 'Todas' | Matricula['status'];

@Component({
  selector: 'app-lista-de-matriculas',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    CurrencyPipe,
    DatePipe,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
  ],
  templateUrl: './lista-de-matriculas.html',
  styleUrl: './lista-de-matriculas.css',
})
export class ListaDeMatriculas {
  private readonly matriculaService = inject(MatriculaService);

  readonly colunas = [
    'pokemon',
    'nivel',
    'treinador',
    'plano',
    'valorMensal',
    'valorPrimeiraCobranca',
    'status',
    'dataInicio',
    'dataFim',
    'acoes',
  ];

  busca = new FormControl('', { nonNullable: true });
  filtroStatus = new FormControl<FiltroStatus>('Todas', { nonNullable: true });

  private readonly buscaValor = toSignal(this.busca.valueChanges, { initialValue: '' });
  private readonly filtroStatusValor = toSignal(this.filtroStatus.valueChanges, {
    initialValue: 'Todas' as FiltroStatus,
  });

  matriculas = signal<Matricula[]>([]);
  carregando = signal(false);
  erro = signal<string | null>(null);
  cancelandoId = signal<number | null>(null);

  matriculasFiltradas = computed(() => {
    const termo = this.buscaValor().trim().toLowerCase();
    const status = this.filtroStatusValor();

    return this.matriculas().filter((matricula) => {
      const combinaStatus = status === 'Todas' || matricula.status === status;
      const combinaBusca =
        !termo ||
        matricula.pokemonNome.toLowerCase().includes(termo) ||
        matricula.treinadorNome.toLowerCase().includes(termo);
      return combinaStatus && combinaBusca;
    });
  });

  constructor() {
    this.carregar();
  }

  carregar(): void {
    this.carregando.set(true);
    this.erro.set(null);

    this.matriculaService.listar().subscribe({
      next: (matriculas) => {
        this.carregando.set(false);
        this.matriculas.set(matriculas);
      },
      error: (erro: HttpErrorResponse) => {
        this.carregando.set(false);
        this.erro.set(erro.error?.mensagem ?? 'Ocorreu um erro inesperado. Tente novamente.');
      },
    });
  }

  cancelar(matricula: Matricula): void {
    this.cancelandoId.set(matricula.id);
    this.erro.set(null);

    this.matriculaService.cancelar(matricula.id).subscribe({
      next: (atualizada) => {
        this.cancelandoId.set(null);
        this.matriculas.update((lista) =>
          lista.map((item) => (item.id === atualizada.id ? atualizada : item)),
        );
      },
      error: (erro: HttpErrorResponse) => {
        this.cancelandoId.set(null);
        this.erro.set(erro.error?.mensagem ?? 'Ocorreu um erro inesperado. Tente novamente.');
      },
    });
  }
}
