import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';
import { FormsModule } from '@angular/forms';

import { MatriculaService } from '../../core/services/matricula.service';
import { PlanoTreinamentoService } from '../../core/services/plano-treinamento.service';
import { PokemonService } from '../../core/services/pokemon.service';

import { MatriculaRequest } from '../../models/matricula/matricula-request';
import { MatriculaResponse } from '../../models/matricula/matricula-response';
import { UpgradeMatriculaRequest } from '../../models/matricula/upgrade-matricula-request';
import { PlanoTreinamentoResponse } from '../../models/plano-treinamento/plano-treinamento-response';
import { PokemonResponse } from '../../models/pokemon/pokemon-response';

@Component({
  selector: 'app-matriculas',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './matriculas.html',
  styleUrl: './matriculas.scss'
})
export class Matriculas implements OnInit {
  matriculas: MatriculaResponse[] = [];
  pokemons: PokemonResponse[] = [];
  planos: PlanoTreinamentoResponse[] = [];

  busca: string = '';
  status?: number;

  carregando: boolean = false;
  salvando: boolean = false;
  cancelandoId?: number;

  mensagemSucesso: string = '';
  mensagemErro: string = '';

  exibirFormulario: boolean = false;

  novaMatricula: MatriculaRequest = {
    pokemonId: 0,
    planoTreinamentoId: 0,
    dataInicio: this.obterDataAtual()
  };

  // Dados do upgrade
  exibirFormularioUpgrade: boolean = false;
  matriculaUpgrade?: MatriculaResponse;

  upgradeRequest: UpgradeMatriculaRequest = {
    novoPlanoTreinamentoId: 0,
    dataUpgrade: this.obterDataAtual()
  };

  simulandoUpgrade: boolean = false;
  confirmandoUpgrade: boolean = false;

  simulacaoUpgrade: any = undefined;

  constructor(
    private readonly matriculaService: MatriculaService,
    private readonly pokemonService: PokemonService,
    private readonly planoTreinamentoService: PlanoTreinamentoService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.carregarMatriculas();
    this.carregarPokemons();
    this.carregarPlanos();
  }

  carregarMatriculas(): void {
    this.carregando = true;
    this.mensagemErro = '';

    this.matriculaService
      .listar(this.busca, this.status)
      .subscribe({
        next: (response: MatriculaResponse[]) => {
          this.matriculas = response;
          this.carregando = false;
          this.changeDetectorRef.detectChanges();
        },
        error: (erro) => {
          this.mensagemErro = this.obterMensagemErro(
            erro,
            'Não foi possível carregar as matrículas.'
          );

          this.carregando = false;
          this.changeDetectorRef.detectChanges();
        }
      });
  }

  carregarPokemons(): void {
    this.pokemonService
      .listar()
      .subscribe({
        next: (response: PokemonResponse[]) => {
          this.pokemons = response;
          this.changeDetectorRef.detectChanges();
        },
        error: (erro) => {
          this.mensagemErro = this.obterMensagemErro(
            erro,
            'Não foi possível carregar os Pokémon.'
          );

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  carregarPlanos(): void {
    this.planoTreinamentoService
      .listar()
      .subscribe({
        next: (response: PlanoTreinamentoResponse[]) => {
          this.planos = response;
          this.changeDetectorRef.detectChanges();
        },
        error: (erro) => {
          this.mensagemErro = this.obterMensagemErro(
            erro,
            'Não foi possível carregar os planos.'
          );

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  limparFiltros(): void {
    this.busca = '';
    this.status = undefined;
    this.carregarMatriculas();
  }

  abrirFormulario(): void {
    this.fecharUpgrade();

    this.novaMatricula = {
      pokemonId: 0,
      planoTreinamentoId: 0,
      dataInicio: this.obterDataAtual()
    };

    this.mensagemErro = '';
    this.mensagemSucesso = '';
    this.exibirFormulario = true;
  }

  fecharFormulario(): void {
    this.exibirFormulario = false;

    this.novaMatricula = {
      pokemonId: 0,
      planoTreinamentoId: 0,
      dataInicio: this.obterDataAtual()
    };
  }

  salvarMatricula(): void {
    this.mensagemErro = '';
    this.mensagemSucesso = '';

    if (this.novaMatricula.pokemonId <= 0) {
      this.mensagemErro = 'Selecione um Pokémon.';
      return;
    }

    if (this.novaMatricula.planoTreinamentoId <= 0) {
      this.mensagemErro = 'Selecione um plano de treinamento.';
      return;
    }

    if (!this.novaMatricula.dataInicio) {
      this.mensagemErro = 'Informe a data de início.';
      return;
    }

    if (!this.validarNivelElite(
      this.novaMatricula.pokemonId,
      this.novaMatricula.planoTreinamentoId
    )) {
      return;
    }

    this.salvando = true;

    this.matriculaService
      .inserir(this.novaMatricula)
      .subscribe({
        next: () => {
          this.salvando = false;
          this.mensagemSucesso = 'Matrícula cadastrada com sucesso.';

          this.fecharFormulario();
          this.carregarMatriculas();
          this.changeDetectorRef.detectChanges();
        },
        error: (erro) => {
          this.salvando = false;

          this.mensagemErro = this.obterMensagemErro(
            erro,
            'Não foi possível cadastrar a matrícula.'
          );

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  cancelarMatricula(matricula: MatriculaResponse): void {
    const desejaCancelar: boolean = window.confirm(
      `Deseja realmente cancelar a matrícula do Pokémon ${matricula.pokemonNome}?`
    );

    if (!desejaCancelar) {
      return;
    }

    this.cancelandoId = matricula.id;
    this.mensagemErro = '';
    this.mensagemSucesso = '';

    this.matriculaService
      .cancelar(matricula.id)
      .subscribe({
        next: () => {
          this.cancelandoId = undefined;
          this.mensagemSucesso = 'Matrícula cancelada com sucesso.';

          this.carregarMatriculas();
          this.changeDetectorRef.detectChanges();
        },
        error: (erro) => {
          this.cancelandoId = undefined;

          this.mensagemErro = this.obterMensagemErro(
            erro,
            'Não foi possível cancelar a matrícula.'
          );

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  abrirUpgrade(matricula: MatriculaResponse): void {
    this.fecharFormulario();

    this.matriculaUpgrade = matricula;

    this.upgradeRequest = {
      novoPlanoTreinamentoId: 0,
      dataUpgrade: this.obterDataAtual()
    };

    this.simulacaoUpgrade = undefined;
    this.mensagemErro = '';
    this.mensagemSucesso = '';
    this.exibirFormularioUpgrade = true;
  }

  fecharUpgrade(): void {
    this.exibirFormularioUpgrade = false;
    this.matriculaUpgrade = undefined;
    this.simulacaoUpgrade = undefined;

    this.upgradeRequest = {
      novoPlanoTreinamentoId: 0,
      dataUpgrade: this.obterDataAtual()
    };
  }

  simularUpgrade(): void {
    this.mensagemErro = '';
    this.mensagemSucesso = '';

    if (!this.matriculaUpgrade) {
      this.mensagemErro = 'Nenhuma matrícula foi selecionada.';
      return;
    }

    if (this.upgradeRequest.novoPlanoTreinamentoId <= 0) {
      this.mensagemErro = 'Selecione o novo plano.';
      return;
    }

    if (!this.upgradeRequest.dataUpgrade) {
      this.mensagemErro = 'Informe a data do upgrade.';
      return;
    }

    if (!this.validarNivelElite(
      this.obterPokemonIdMatricula(this.matriculaUpgrade),
      this.upgradeRequest.novoPlanoTreinamentoId
    )) {
      return;
    }

    this.simulandoUpgrade = true;
    this.simulacaoUpgrade = undefined;

    this.matriculaService
      .simularUpgrade(
        this.matriculaUpgrade.id,
        this.upgradeRequest
      )
      .subscribe({
        next: (response) => {
          this.simulacaoUpgrade = response;
          this.simulandoUpgrade = false;
          this.changeDetectorRef.detectChanges();
        },
        error: (erro) => {
          this.simulandoUpgrade = false;

          this.mensagemErro = this.obterMensagemErro(
            erro,
            'Não foi possível simular o upgrade.'
          );

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  confirmarUpgrade(): void {
    this.mensagemErro = '';
    this.mensagemSucesso = '';

    if (!this.matriculaUpgrade) {
      this.mensagemErro = 'Nenhuma matrícula foi selecionada.';
      return;
    }

    if (!this.simulacaoUpgrade) {
      this.mensagemErro =
        'Faça a simulação antes de confirmar o upgrade.';

      return;
    }

    this.confirmandoUpgrade = true;

    this.matriculaService
      .confirmarUpgrade(
        this.matriculaUpgrade.id,
        this.upgradeRequest
      )
      .subscribe({
        next: () => {
          this.confirmandoUpgrade = false;
          this.mensagemSucesso = 'Upgrade realizado com sucesso.';

          this.fecharUpgrade();
          this.carregarMatriculas();
          this.changeDetectorRef.detectChanges();
        },
        error: (erro) => {
          this.confirmandoUpgrade = false;

          this.mensagemErro = this.obterMensagemErro(
            erro,
            'Não foi possível confirmar o upgrade.'
          );

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  obterNomeStatus(status: number): string {
    switch (status) {
      case 1:
        return 'Ativa';

      case 2:
        return 'Cancelada';

      case 3:
        return 'Concluída';

      default:
        return 'Desconhecido';
    }
  }

  obterValorPrimeiraCobranca(): number | undefined {
    if (!this.simulacaoUpgrade) {
      return undefined;
    }

    return (
      this.simulacaoUpgrade.valorPrimeiraCobranca ??
      this.simulacaoUpgrade.primeiraCobranca ??
      this.simulacaoUpgrade.valorCobrar ??
      this.simulacaoUpgrade.valorProporcional
    );
  }

  private validarNivelElite(
    pokemonId: number,
    planoId: number
  ): boolean {
    const pokemon: PokemonResponse | undefined =
      this.pokemons.find(
        (item: PokemonResponse) => item.id === pokemonId
      );

    const plano: PlanoTreinamentoResponse | undefined =
      this.planos.find(
        (item: PlanoTreinamentoResponse) => item.id === planoId
      );

    if (!pokemon || !plano) {
      return true;
    }

    const nomePlano: string = plano.nome
      .trim()
      .toLowerCase();

    const planoElite: boolean =
      nomePlano.includes('elite dos 4');

    if (planoElite && pokemon.nivel < 50) {
      this.mensagemErro =
        'Somente Pokémon de nível 50 ou superior podem utilizar o plano Elite dos 4.';

      return false;
    }

    return true;
  }

  private obterPokemonIdMatricula(
    matricula: MatriculaResponse
  ): number {
    const matriculaComPokemonId =
      matricula as MatriculaResponse & {
        pokemonId?: number;
      };

    return matriculaComPokemonId.pokemonId ?? 0;
  }

  private obterDataAtual(): string {
    const dataAtual: Date = new Date();

    const ano: number = dataAtual.getFullYear();
    const mes: string = String(
      dataAtual.getMonth() + 1
    ).padStart(2, '0');

    const dia: string = String(
      dataAtual.getDate()
    ).padStart(2, '0');

    return `${ano}-${mes}-${dia}`;
  }

  private obterMensagemErro(
    erro: any,
    mensagemPadrao: string
  ): string {
    return (
      erro?.error?.mensagem ??
      erro?.error?.message ??
      erro?.error?.title ??
      mensagemPadrao
    );
  }
}