import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { debounceTime, startWith, switchMap } from 'rxjs';

import { Enrollment, ENROLLMENT_STATUS_LABELS, EnrollmentStatus } from '../../shared/models/enrollment.model';
import { EnrollmentApiService } from '../../shared/services/enrollment-api.service';

@Component({
  selector: 'app-enrollments-list',
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './enrollments-list.html',
  styleUrl: './enrollments-list.css',
})
export class EnrollmentsList implements OnInit {
  private readonly enrollmentApi = inject(EnrollmentApiService);
  private readonly router = inject(Router);

  readonly statusLabels = ENROLLMENT_STATUS_LABELS;
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly statusControl = new FormControl<EnrollmentStatus | ''>('', { nonNullable: true });

  // em change detection zoneless (ver app.config.ts), usamos signals, não campos soltos
  readonly enrollments = signal<Enrollment[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  // pega a mensagem de sucesso que a tela anterior possa ter mandado durante o redirecionamento
  // (ver this.router.navigate no enrollment-form.ts)
  readonly successMessage = signal<string | null>(
    (this.router.getCurrentNavigation()?.extras.state as { successMessage?: string } | undefined)?.successMessage ?? null
  );

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(
        // faz uma primeira emissão com uma string vazia ('') para disparar a busca inicial
        startWith(this.searchControl.value),
        debounceTime(300),
        switchMap((search) => this.fetch(search, this.statusControl.value))
      )
      .subscribe(); // garante que o Observable do fetch() seja disparado

    this.statusControl.valueChanges
      .pipe(switchMap((status) => this.fetch(this.searchControl.value, status)))
      .subscribe();
  }

  /** FR-012: cancelamento não estorna o valor já pago — aviso explícito antes de confirmar. */
  cancel(enrollment: Enrollment): void {
    const confirmed = confirm(
      `Cancelar a matrícula de ${enrollment.pokemonName}? O acesso continua até o fim do ciclo pago atual, sem estorno do valor já pago.`
    );
    if (!confirmed) {
      return;
    }

    this.errorMessage.set(null);
    this.enrollmentApi.cancel(enrollment.id).subscribe({
      next: () => this.fetch(this.searchControl.value, this.statusControl.value).subscribe(),
      error: (err: Error) => this.errorMessage.set(err.message),
    });
  }

  private fetch(search: string, status: EnrollmentStatus | '') {
    this.loading.set(true);
    return this.enrollmentApi.list(search || undefined, status || undefined).pipe(
      switchMap((enrollments) => {
        this.enrollments.set(enrollments);
        this.loading.set(false);
        return [enrollments];
      })
    );
  }
}
