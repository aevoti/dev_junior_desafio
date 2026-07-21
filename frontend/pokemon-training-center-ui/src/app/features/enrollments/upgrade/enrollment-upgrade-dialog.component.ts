import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EnrollmentService } from '../../../core/services/enrollment.service';
import { PlanService } from '../../../core/services/plan.service';
import { Enrollment } from '../../../shared/models/enrollment.model';
import { Plan } from '../../../shared/models/plan.model';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-enrollment-upgrade-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './enrollment-upgrade-dialog.component.html',
  styleUrl: './enrollment-upgrade-dialog.component.scss'
})
export class EnrollmentUpgradeDialogComponent implements OnInit {
  @Input({ required: true }) enrollment!: Enrollment;
  @Output() close = new EventEmitter<void>();
  @Output() confirmed = new EventEmitter<void>();

  plans = signal<Plan[]>([]);
  planoSelecionadoId: number | null = null;
  dataUpgrade = new Date().toISOString().slice(0, 10);
  simulacao = signal<{ valorPrimeiraCobranca: number; diasRestantes: number } | null>(null);
  simulando = signal(false);
  confirmando = signal(false);

  constructor(
    private planService: PlanService,
    private enrollmentService: EnrollmentService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.planService.getAll().subscribe(data => {
      this.plans.set(data.filter(p => p.valorMensal > this.enrollment.valorMensal));
    });
  }

  simular(): void {
    if (!this.planoSelecionadoId) return;

    this.simulando.set(true);
    this.enrollmentService
      .simulateUpgrade(this.enrollment.id, {
        novoPlanoId: this.planoSelecionadoId,
        dataUpgrade: this.dataUpgrade
      })
      .subscribe({
        next: resultado => {
          this.simulacao.set(resultado);
          this.simulando.set(false);
        },
        error: () => this.simulando.set(false)
      });
  }

  confirmar(): void {
    if (!this.planoSelecionadoId) return;

    this.confirmando.set(true);
    this.enrollmentService
      .confirmUpgrade(this.enrollment.id, {
        novoPlanoId: this.planoSelecionadoId,
        dataUpgrade: this.dataUpgrade
      })
      .subscribe({
        next: () => {
          this.toastService.show('Upgrade confirmado com sucesso!', 'success');
          this.confirmed.emit();
        },
        error: () => this.confirmando.set(false)
      });
  }
}