import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EnrollmentService } from '../../../core/services/enrollment.service';
import { Enrollment } from '../../../shared/models/enrollment.model';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-enrollment-cancel-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './enrollment-cancel-dialog.component.html',
  styleUrl: './enrollment-cancel-dialog.component.scss'
})
export class EnrollmentCancelDialogComponent {

  @Input({ required: true })
  enrollment!: Enrollment;

  @Output()
  close = new EventEmitter<void>();

  @Output()
  confirmed = new EventEmitter<void>();

  cancelando = signal(false);

  constructor(
    private enrollmentService: EnrollmentService,
    private toastService: ToastService
  ) {}

  confirmar(): void {
    this.cancelando.set(true);

    this.enrollmentService.cancel(this.enrollment.id).subscribe({
      next: () => {
        this.toastService.show('Matrícula cancelada com sucesso!', 'success');
        this.confirmed.emit();
      },
      error: () => this.cancelando.set(false)
    });
  }
}