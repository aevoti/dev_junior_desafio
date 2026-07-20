import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { MatriculaService } from '../../../../core/services/matricula.service';
import { PlanoTreinamentoService } from '../../../../core/services/plano-treinamento.service';
import { PokemonService } from '../../../../core/services/pokemon.service';
import { PlanoTreinamento } from '../../../../core/models/plano-treinamento.model';
import { Pokemon } from '../../../../core/models/pokemon.model';

const NIVEL_MINIMO_ELITE_DOS_4 = 50;

/**
 * Tela 2 (formulário): validações de campos obrigatórios e nível mínimo
 * para o plano Elite dos 4 (R3). A validação aqui é só UX — a regra
 * definitiva é sempre reforçada pela API (ver MatriculaService.CriarAsync).
 */
@Component({
  selector: 'app-matricula-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './matricula-form.component.html',
  styleUrl: './matricula-form.component.scss',
})
export class MatriculaFormComponent implements OnInit {
  private readonly matriculaService = inject(MatriculaService);
  private readonly planoService = inject(PlanoTreinamentoService);
  private readonly pokemonService = inject(PokemonService);
  private readonly router = inject(Router);

  planos: PlanoTreinamento[] = [];
  pokemons: Pokemon[] = [];
  erroApi: string | null = null;
  carregandoPokemons = false;
  enviando = false;

  pokemonId: number | null = null;
  planoTreinamentoId: number | null = null;
  dataInicio: string = new Date().toISOString().substring(0, 10);

  ngOnInit(): void {
    this.planoService.listar().subscribe((planos) => (this.planos = planos));

    this.carregandoPokemons = true;
    this.pokemonService.listar().subscribe({
      next: (pokemons) => {
        this.pokemons = pokemons;
        this.carregandoPokemons = false;
      },
      error: () => (this.carregandoPokemons = false),
    });
  }

  get pokemonSelecionado(): Pokemon | undefined {
    return this.pokemons.find((p) => p.id === this.pokemonId);
  }

  get nivelInsuficiente(): boolean {
    const plano = this.planos.find((p) => p.id === this.planoTreinamentoId);
    const pokemon = this.pokemonSelecionado;
    if (!plano || !pokemon) return false;
    return pokemon.nivel < plano.nivelMinimoPokemon;
  }

  submeter(form: NgForm): void {
    this.erroApi = null;
    if (form.invalid || this.nivelInsuficiente || !this.pokemonId || !this.planoTreinamentoId) {
      return;
    }

    this.enviando = true;
    this.matriculaService
      .criar({
        pokemonId: this.pokemonId,
        planoTreinamentoId: this.planoTreinamentoId,
        dataInicio: this.dataInicio,
      })
      .subscribe({
        next: () => this.router.navigate(['/matriculas']),
        error: (err: Error) => {
          // R1: mensagem amigável quando já existe matrícula ativa para o Pokémon.
          this.erroApi = err.message;
          this.enviando = false;
        },
      });
  }

  protected readonly NIVEL_MINIMO_ELITE_DOS_4 = NIVEL_MINIMO_ELITE_DOS_4;
}
