import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';
import { RouterLink } from '@angular/router';

import { MatriculaService } from '../../core/services/matricula.service';
import { PlanoTreinamentoService } from '../../core/services/plano-treinamento.service';
import { PokemonService } from '../../core/services/pokemon.service';
import { TreinadorService } from '../../core/services/treinador.service';

import { MatriculaResponse } from '../../models/matricula/matricula-response';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {

  totalTreinadores: number = 0;
  totalPokemons: number = 0;
  matriculasAtivas: number = 0;
  totalPlanos: number = 0;

  ultimasMatriculas: MatriculaResponse[] = [];

  constructor(
    private readonly treinadorService: TreinadorService,
    private readonly pokemonService: PokemonService,
    private readonly matriculaService: MatriculaService,
    private readonly planoTreinamentoService: PlanoTreinamentoService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {
  }

  ngOnInit(): void {
    this.carregarResumo();
  }

  private carregarResumo(): void {
    this.carregarTreinadores();
    this.carregarPokemons();
    this.carregarMatriculas();
    this.carregarPlanos();
  }

  private carregarTreinadores(): void {
    this.treinadorService
      .listar('')
      .subscribe({
        next: (response) => {
          this.totalTreinadores = response.length;
          this.changeDetectorRef.detectChanges();
        }
      });
  }

  private carregarPokemons(): void {
    this.pokemonService
      .listar('')
      .subscribe({
        next: (response) => {
          this.totalPokemons = response.length;
          this.changeDetectorRef.detectChanges();
        }
      });
  }

  private carregarMatriculas(): void {
    this.matriculaService
      .listar('', undefined)
      .subscribe({
        next: (response: MatriculaResponse[]) => {
          this.matriculasAtivas = response.filter(
            (matricula: MatriculaResponse) => matricula.status === 1
          ).length;

          this.ultimasMatriculas = response
            .slice()
            .sort(
              (
                primeira: MatriculaResponse,
                segunda: MatriculaResponse
              ) =>
                new Date(segunda.dataInicio).getTime() -
                new Date(primeira.dataInicio).getTime()
            )
            .slice(0, 5);

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  private carregarPlanos(): void {
    this.planoTreinamentoService
      .listar()
      .subscribe({
        next: (response) => {
          this.totalPlanos = response.length;
          this.changeDetectorRef.detectChanges();
        }
      });
  }
}