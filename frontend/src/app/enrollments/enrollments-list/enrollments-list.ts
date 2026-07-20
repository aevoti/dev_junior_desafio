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

  // signal(), não campos soltos: em change detection zoneless (ver
  // app.config.ts), só signals/eventos DOM já rastreados pelo Angular
  // disparam re-render sozinhos — um campo mutado dentro de .subscribe()
  // ficava "invisível" até o usuário clicar em algo na tela.
  readonly enrollments = signal<Enrollment[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  // FR-023: mensagem de sucesso vinda do redirecionamento pós-cadastro (router state).
  readonly successMessage = signal<string | null>(
    (this.router.getCurrentNavigation()?.extras.state as { successMessage?: string } | undefined)?.successMessage ?? null
  );

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(
        startWith(this.searchControl.value),
        debounceTime(300),
        switchMap((search) => this.fetch(search, this.statusControl.value))
      )
      .subscribe();

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
      // Atualiza a lista assim que a resposta chega; erros já viram alerta amigável via interceptor.
      // (mantido simples: sem tratamento de erro dedicado nesta tela de listagem)
      switchMap((enrollments) => {
        this.enrollments.set(enrollments);
        this.loading.set(false);
        return [enrollments];
      })
    );
  }
}
