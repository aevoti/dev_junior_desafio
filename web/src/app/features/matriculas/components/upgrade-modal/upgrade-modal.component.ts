import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatriculaService } from '../../../../core/services/matricula.service';
import { PlanoTreinamento } from '../../../../core/models/plano-treinamento.model';
import { UpgradeMatriculaResponse } from '../../../../core/models/matricula.model';

/**
 * Modal do fluxo de upgrade (R2): "Calcular" só simula (sem side-effect) e mostra
 * o valor da primeira cobrança pro-rata; o upgrade só é efetivado de fato quando
 * o usuário clica em "Confirmar upgrade".
 */
@Component({
  selector: 'app-upgrade-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './upgrade-modal.component.html',
  styleUrl: './upgrade-modal.component.scss',
})
export class UpgradeModalComponent {
  private readonly matriculaService = inject(MatriculaService);

  @Input({ required: true }) matriculaId!: number;
  @Input() planosDisponiveis: PlanoTreinamento[] = [];
  @Output() confirmado = new EventEmitter<void>();
  @Output() fechado = new EventEmitter<void>();

  simulacao: UpgradeMatriculaResponse | null = null;
  erro: string | null = null;
  planoSelecionadoId: number | null = null;
  confirmando = false;

  private dataUpgrade = new Date().toISOString().substring(0, 10);

  simular(): void {
    if (!this.planoSelecionadoId) return;
    this.erro = null;
    this.simulacao = null;
    this.matriculaService
      .simularUpgrade(this.matriculaId, {
        novoPlanoTreinamentoId: this.planoSelecionadoId,
        dataUpgrade: this.dataUpgrade,
      })
      .subscribe({
        next: (resultado) => (this.simulacao = resultado),
        error: (err: Error) => (this.erro = err.message),
      });
  }

  confirmar(): void {
    if (!this.planoSelecionadoId) return;
    this.erro = null;
    this.confirmando = true;
    this.matriculaService
      .upgrade(this.matriculaId, {
        novoPlanoTreinamentoId: this.planoSelecionadoId,
        dataUpgrade: this.dataUpgrade,
      })
      .subscribe({
        next: () => {
          this.confirmando = false;
          this.confirmado.emit();
        },
        error: (err: Error) => {
          this.erro = err.message;
          this.confirmando = false;
        },
      });
  }
}
