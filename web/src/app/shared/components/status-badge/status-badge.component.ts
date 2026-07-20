import { Component, Input } from '@angular/core';
import { StatusMatricula } from '../../../core/models/matricula.model';

const VARIANTS: Record<StatusMatricula, string> = {
  Ativa: 'success',
  Cancelada: 'danger',
  Concluida: 'info',
};

const LABELS: Record<StatusMatricula, string> = {
  Ativa: 'Ativa',
  Cancelada: 'Cancelada',
  Concluida: 'Concluída',
};

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `<span class="badge" [class]="'badge-' + variant">{{ label }}</span>`,
})
export class StatusBadgeComponent {
  @Input({ required: true }) status!: StatusMatricula;

  get variant(): string {
    return VARIANTS[this.status] ?? 'neutral';
  }

  get label(): string {
    return LABELS[this.status] ?? this.status;
  }
}
