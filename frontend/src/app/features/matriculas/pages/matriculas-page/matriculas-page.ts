import { CurrencyPipe } from '@angular/common';
import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { forkJoin, finalize } from 'rxjs';
import { Matricula, PlanoTreinamento, Pokemon, StatusMatricula } from '../../../../core/models/api.models';
import { obterMensagemErro } from '../../../../core/services/api-error';
import { MatriculaService } from '../../../../core/services/matricula.service';
import { PlanoService } from '../../../../core/services/plano.service';
import { PokemonService } from '../../../../core/services/pokemon.service';
import { MatriculasList } from '../../components/matriculas-list/matriculas-list';
import { NovaMatriculaForm } from '../../components/nova-matricula-form/nova-matricula-form';
import { UpgradeMatricula } from '../../components/upgrade-matricula/upgrade-matricula';

@Component({
  selector: 'app-matriculas-page',
  imports: [ReactiveFormsModule, CurrencyPipe, MatriculasList, NovaMatriculaForm, UpgradeMatricula],
  templateUrl: './matriculas-page.html',
  styleUrl: './matriculas-page.scss'
})
export class MatriculasPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly matriculaService = inject(MatriculaService);
  private readonly pokemonService = inject(PokemonService);
  private readonly planoService = inject(PlanoService);
  private readonly cdr = inject(ChangeDetectorRef);

  matriculas: Matricula[] = [];
  pokemons: Pokemon[] = [];
  planos: PlanoTreinamento[] = [];
  carregando = false;
  mostrarNova = false;
  matriculaUpgrade: Matricula | null = null;
  sucesso = '';
  erro = '';
  readonly filtros = this.fb.nonNullable.group({ busca: '', status: '' as StatusMatricula | '' });

  get matriculasAtivas(): number { return this.matriculas.filter(item => item.status === 'Ativa').length; }
  get matriculasCanceladas(): number { return this.matriculas.filter(item => item.status === 'Cancelada').length; }
  get matriculasConcluidas(): number { return this.matriculas.filter(item => item.status === 'Concluida').length; }
  get receitaMensalAtiva(): number {
    return this.matriculas
      .filter(item => item.status === 'Ativa')
      .reduce((total, item) => total + item.valorMensal, 0);
  }

  ngOnInit(): void {
    this.carregando = true;
    forkJoin({
      matriculas: this.matriculaService.listar(),
      pokemons: this.pokemonService.listar(),
      planos: this.planoService.listar()
    }).pipe(finalize(() => { this.carregando = false; this.cdr.markForCheck(); })).subscribe({
      next: dados => { this.matriculas = dados.matriculas; this.pokemons = dados.pokemons; this.planos = dados.planos; this.cdr.markForCheck(); },
      error: error => this.mostrarErro(obterMensagemErro(error))
    });
  }

  aplicarFiltros(): void {
    const { busca, status } = this.filtros.getRawValue();
    this.carregarMatriculas(busca, status);
  }

  limparFiltros(): void {
    this.filtros.reset({ busca: '', status: '' });
    this.carregarMatriculas();
  }

  matriculaCriada(): void {
    this.mostrarNova = false;
    this.sucesso = 'Matrícula criada com sucesso.';
    this.erro = '';
    this.carregarTudoAuxiliar();
  }

  upgradeConcluido(): void {
    this.matriculaUpgrade = null;
    this.sucesso = 'Upgrade realizado com sucesso.';
    this.erro = '';
    this.carregarMatriculas();
  }

  mostrarErro(message: string): void { this.erro = message; this.sucesso = ''; }

  private carregarMatriculas(busca = '', status: StatusMatricula | '' = ''): void {
    this.carregando = true;
    this.matriculaService.listar(busca, status).pipe(finalize(() => { this.carregando = false; this.cdr.markForCheck(); })).subscribe({
      next: matriculas => { this.matriculas = matriculas; this.cdr.markForCheck(); },
      error: error => this.mostrarErro(obterMensagemErro(error))
    });
  }

  private carregarTudoAuxiliar(): void {
    forkJoin({ matriculas: this.matriculaService.listar(), pokemons: this.pokemonService.listar() }).subscribe({
      next: dados => { this.matriculas = dados.matriculas; this.pokemons = dados.pokemons; this.cdr.markForCheck(); },
      error: error => this.mostrarErro(obterMensagemErro(error))
    });
  }
}
