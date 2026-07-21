import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EnrollmentService } from '../../../core/services/enrollment.service';
import { Enrollment } from '../../../shared/models/enrollment.model';
import { EnrollmentStatus } from '../../../shared/models/enrollment-status.model';
import { ParallaxHeroComponent } from '../../../shared/components/parallax-hero/parallax-hero.component';
import { EnrollmentUpgradeDialogComponent } from '../upgrade/enrollment-upgrade-dialog.component';
import { EnrollmentCancelDialogComponent } from '../cancel/enrollment-cancel-dialog.component';

@Component({
  selector: 'app-enrollment-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, ParallaxHeroComponent, EnrollmentUpgradeDialogComponent, EnrollmentCancelDialogComponent],
  templateUrl: './enrollment-list.component.html',
  styleUrl: './enrollment-list.component.scss'
})
export class EnrollmentListComponent implements OnInit {
  enrollments = signal<Enrollment[]>([]);
  loading = signal(true);
  busca = '';
  statusFiltro: EnrollmentStatus | '' = '';
  upgradeAlvo = signal<Enrollment | null>(null);
  cancelamentoAlvo = signal<Enrollment | null>(null);

  constructor(private enrollmentService: EnrollmentService) { }

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.loading.set(true);
    this.enrollmentService
      .getAll(this.busca || undefined, this.statusFiltro || undefined)
      .subscribe({
        next: data => {
          this.enrollments.set(data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }

  abrirUpgrade(enrollment: Enrollment): void {
    this.upgradeAlvo.set(enrollment);
  }

  fecharUpgrade(): void {
    this.upgradeAlvo.set(null);
  }

  aoConfirmarUpgrade(): void {
    this.fecharUpgrade();
    this.carregar();
  }

  abrirCancelamento(enrollment: Enrollment): void {
    this.cancelamentoAlvo.set(enrollment);
  }

  fecharCancelamento(): void {
    this.cancelamentoAlvo.set(null);
  }

  aoConfirmarCancelamento(): void {
    const enrollment = this.cancelamentoAlvo();

    if (!enrollment) {
      return;
    }

    this.enrollmentService.cancel(enrollment.id).subscribe({
      next: () => {
        this.fecharCancelamento();
        this.carregar();
      }
    });
  }

  statusClass(status: EnrollmentStatus): string {
    return {
      'Ativa': 'status--ativa',
      'Cancelada': 'status--cancelada',
      'Concluida': 'status--concluida'
    }[status];
  }
}