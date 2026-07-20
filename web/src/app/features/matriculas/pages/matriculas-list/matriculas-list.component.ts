import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatriculaService } from '../../../../core/services/matricula.service';
import { PlanoTreinamentoService } from '../../../../core/services/plano-treinamento.service';
import { PokemonService } from '../../../../core/services/pokemon.service';
import { Matricula, StatusMatricula } from '../../../../core/models/matricula.model';
import { PlanoTreinamento } from '../../../../core/models/plano-treinamento.model';
import { Pokemon } from '../../../../core/models/pokemon.model';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { UpgradeModalComponent } from '../../components/upgrade-modal/upgrade-modal.component';
import { TransferirModalComponent } from '../../components/transferir-modal/transferir-modal.component';

/** Tela 1 (listagem): busca por nome do Pokémon/Treinador + filtro por status,
 * ação de upgrade (R2), cancelamento (R4) e transferência de Treinador (R5) por matrícula. */
@Component({
  selector: 'app-matriculas-list',
  standalone: true,
  imports: [CommonModule, FormsModule, StatusBadgeComponent, UpgradeModalComponent, TransferirModalComponent],
  templateUrl: './matriculas-list.component.html',
})
export class MatriculasListComponent implements OnInit {
  private readonly matriculaService = inject(MatriculaService);
  private readonly planoService = inject(PlanoTreinamentoService);
  private readonly pokemonService = inject(PokemonService);

  matriculas: Matricula[] = [];
  planos: PlanoTreinamento[] = [];
  pokemons: Pokemon[] = [];
  busca = '';
  statusSelecionado: StatusMatricula | '' = '';
  carregando = false;
  erro: string | null = null;

  matriculaEmUpgrade: Matricula | null = null;
  matriculaEmTransferencia: Matricula | null = null;
  cancelandoId: number | null = null;

  ngOnInit(): void {
    this.buscar();
    this.planoService.listar().subscribe((planos) => (this.planos = planos));
    this.carregarPokemons();
  }

  private carregarPokemons(): void {
    this.pokemonService.listar().subscribe((pokemons) => (this.pokemons = pokemons));
  }

  treinadorAtualId(matricula: Matricula): number | undefined {
    return this.pokemons.find((p) => p.id === matricula.pokemonId)?.treinadorId;
  }

  buscar(): void {
    this.carregando = true;
    this.erro = null;
    this.matriculaService.listar(this.busca || undefined, this.statusSelecionado || undefined).subscribe({
      next: (matriculas) => {
        this.matriculas = matriculas;
        this.carregando = false;
      },
      error: (err: Error) => {
        this.erro = err.message;
        this.carregando = false;
      },
    });
  }

  planosSuperiores(matricula: Matricula): PlanoTreinamento[] {
    const planoAtual = this.planos.find((p) => p.id === matricula.planoTreinamentoId);
    if (!planoAtual) return [];
    return this.planos.filter((p) => p.nivel > planoAtual.nivel);
  }

  abrirUpgrade(matricula: Matricula): void {
    this.erro = null;
    this.matriculaEmUpgrade = matricula;
  }

  onUpgradeConfirmado(): void {
    this.matriculaEmUpgrade = null;
    this.buscar();
  }

  cancelar(matricula: Matricula): void {
    if (!confirm(`Cancelar a matrícula de ${matricula.pokemonNome}?`)) return;

    this.erro = null;
    this.cancelandoId = matricula.id;
    this.matriculaService.cancelar(matricula.id).subscribe({
      next: () => {
        this.cancelandoId = null;
        this.buscar();
      },
      error: (err: Error) => {
        this.erro = err.message;
        this.cancelandoId = null;
      },
    });
  }

  abrirTransferencia(matricula: Matricula): void {
    this.erro = null;
    this.matriculaEmTransferencia = matricula;
  }

  onTransferenciaConfirmada(): void {
    this.matriculaEmTransferencia = null;
    this.buscar();
    this.carregarPokemons();
  }
}
