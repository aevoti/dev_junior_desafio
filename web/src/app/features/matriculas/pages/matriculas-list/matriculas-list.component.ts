import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatriculaService } from '../../../../core/services/matricula.service';
import { PlanoTreinamentoService } from '../../../../core/services/plano-treinamento.service';
import { Matricula, StatusMatricula } from '../../../../core/models/matricula.model';
import { PlanoTreinamento } from '../../../../core/models/plano-treinamento.model';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { UpgradeModalComponent } from '../../components/upgrade-modal/upgrade-modal.component';

/** Tela 1 (listagem): busca por nome do Pokémon/Treinador + filtro por status,
 * ação de upgrade (R2) e cancelamento (R4) por matrícula. */
@Component({
  selector: 'app-matriculas-list',
  standalone: true,
  imports: [CommonModule, FormsModule, StatusBadgeComponent, UpgradeModalComponent],
  templateUrl: './matriculas-list.component.html',
})
export class MatriculasListComponent implements OnInit {
  private readonly matriculaService = inject(MatriculaService);
  private readonly planoService = inject(PlanoTreinamentoService);

  matriculas: Matricula[] = [];
  planos: PlanoTreinamento[] = [];
  busca = '';
  statusSelecionado: StatusMatricula | '' = '';
  carregando = false;
  erro: string | null = null;

  matriculaEmUpgrade: Matricula | null = null;
  cancelandoId: number | null = null;

  ngOnInit(): void {
    this.buscar();
    this.planoService.listar().subscribe((planos) => (this.planos = planos));
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
}
