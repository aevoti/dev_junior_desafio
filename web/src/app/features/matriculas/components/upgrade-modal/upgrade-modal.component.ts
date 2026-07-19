import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatriculaService } from '../../../../core/services/matricula.service';
import { PlanoTreinamento } from '../../../../core/models/plano-treinamento.model';
import { UpgradeMatriculaResponse } from '../../../../core/models/matricula.model';

/**
 * Modal do fluxo de upgrade (R2): calcula e exibe o valor da primeira
 * cobrança pro-rata retornado pela API antes do usuário confirmar.
 * TODO: acionar a partir de um botão "Upgrade" na matriculas-list.component
 * e tratar erro de downgrade / nível mínimo (R3) de forma amigável.
 */
@Component({
  selector: 'app-upgrade-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './upgrade-modal.component.html',
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

  // NOTA: hoje POST /matriculas/{id}/upgrade já efetiva o upgrade ao mesmo tempo
  // que calcula o valor. Para a UX pedida ("exibir valor antes de confirmar"),
  // o próximo passo é separar em endpoint de simulação (GET, sem side-effect)
  // + endpoint de confirmação (POST), documentado no README como melhoria futura.
  simular(): void {
    if (!this.planoSelecionadoId) return;
    this.erro = null;
    this.matriculaService
      .upgrade(this.matriculaId, {
        novoPlanoTreinamentoId: this.planoSelecionadoId,
        dataUpgrade: new Date().toISOString().substring(0, 10),
      })
      .subscribe({
        next: (resultado) => (this.simulacao = resultado),
        error: (err: Error) => (this.erro = err.message),
      });
  }

  confirmar(): void {
    this.confirmado.emit();
  }
}
