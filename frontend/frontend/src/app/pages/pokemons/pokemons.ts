import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';
import { FormsModule } from '@angular/forms';

import { PokemonService } from '../../core/services/pokemon.service';
import { TreinadorService } from '../../core/services/treinador.service';

import { PokemonRequest } from '../../models/pokemon/pokemon-request';
import { PokemonResponse } from '../../models/pokemon/pokemon-response';
import { TreinadorResponse } from '../../models/treinador/treinador-response';

@Component({
  selector: 'app-pokemons',
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './pokemons.html',
  styleUrl: './pokemons.scss'
})
export class Pokemons implements OnInit {
  pokemons: PokemonResponse[] = [];
  treinadores: TreinadorResponse[] = [];

  busca: string = '';

  nome: string = '';
  tipo?: number;
  nivel?: number;
  treinadorId?: number;

  carregando: boolean = false;
  salvando: boolean = false;
  mostrarFormulario: boolean = false;

  mensagemErro: string = '';
  mensagemSucesso: string = '';

  constructor(
    private readonly pokemonService: PokemonService,
    private readonly treinadorService: TreinadorService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.carregarPokemons();
    this.carregarTreinadores();
  }

  carregarPokemons(): void {
    this.carregando = true;
    this.mensagemErro = '';

    this.pokemonService
      .listar(this.busca)
      .subscribe({
        next: (pokemons: PokemonResponse[]) => {
          this.pokemons = pokemons;
          this.carregando = false;

          this.changeDetectorRef.detectChanges();
        },
        error: (erro: HttpErrorResponse) => {
          console.error('Erro ao carregar Pokémon:', erro);

          this.mensagemErro =
            'Não foi possível carregar os Pokémon.';

          this.carregando = false;

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  carregarTreinadores(): void {
    this.treinadorService
      .listar()
      .subscribe({
        next: (treinadores: TreinadorResponse[]) => {
          this.treinadores = treinadores;

          this.changeDetectorRef.detectChanges();
        },
        error: (erro: HttpErrorResponse) => {
          console.error('Erro ao carregar treinadores:', erro);

          this.mensagemErro =
            'Não foi possível carregar os treinadores.';

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  salvarPokemon(): void {
    this.mensagemErro = '';
    this.mensagemSucesso = '';

    if (
      !this.nome.trim() ||
      this.tipo === undefined ||
      this.nivel === undefined ||
      this.treinadorId === undefined
    ) {
      this.mensagemErro =
        'Preencha todos os campos do Pokémon.';

      return;
    }

    if (this.nivel < 1 || this.nivel > 100) {
      this.mensagemErro =
        'O nível do Pokémon deve estar entre 1 e 100.';

      return;
    }

    const request: PokemonRequest = {
      nome: this.nome.trim(),
      tipo: this.tipo,
      nivel: this.nivel,
      treinadorId: this.treinadorId
    };

    this.salvando = true;

    this.pokemonService
      .inserir(request)
      .subscribe({
        next: () => {
          this.mensagemSucesso =
            'Pokémon cadastrado com sucesso.';

          this.salvando = false;
          this.mostrarFormulario = false;

          this.limparFormulario();
          this.carregarPokemons();

          this.changeDetectorRef.detectChanges();
        },
        error: (erro: HttpErrorResponse) => {
          console.error('Erro ao cadastrar Pokémon:', erro);

          this.mensagemErro =
            erro.error?.mensagem ||
            erro.error?.message ||
            'Não foi possível cadastrar o Pokémon.';

          this.salvando = false;

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  abrirFormulario(): void {
    this.mostrarFormulario = true;
    this.mensagemErro = '';
    this.mensagemSucesso = '';
  }

  fecharFormulario(): void {
    this.mostrarFormulario = false;
    this.limparFormulario();
  }

  limparFiltros(): void {
    this.busca = '';
    this.carregarPokemons();
  }

  obterNomeTipo(tipo: number): string {
  switch (tipo) {
    case 1:
      return 'Normal';

    case 2:
      return 'Fogo';

    case 3:
      return 'Água';

    case 4:
      return 'Planta';

    case 5:
      return 'Elétrico';

    case 6:
      return 'Gelo';

    case 7:
      return 'Lutador';

    case 8:
      return 'Veneno';

    case 9:
      return 'Terra';

    case 10:
      return 'Voador';

    case 11:
      return 'Psíquico';

    case 12:
      return 'Inseto';

    case 13:
      return 'Pedra';

    case 14:
      return 'Fantasma';

    case 15:
      return 'Dragão';

    case 16:
      return 'Sombrio';

    case 17:
      return 'Aço';

    case 18:
      return 'Fada';

    default:
      return 'Desconhecido';
  }
}

  private limparFormulario(): void {
    this.nome = '';
    this.tipo = undefined;
    this.nivel = undefined;
    this.treinadorId = undefined;
  }
}