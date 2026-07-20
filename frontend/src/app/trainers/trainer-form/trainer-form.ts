import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

import { TrainerApiService } from '../../shared/services/trainer-api.service';
import { emailWithTldValidator } from '../../shared/validators/email-with-tld.validator';

@Component({
  selector: 'app-trainer-form',
  imports: [ReactiveFormsModule],
  templateUrl: './trainer-form.html',
  styleUrl: './trainer-form.css',
})
export class TrainerForm {
  private readonly fb = inject(FormBuilder);
  private readonly trainerApi = inject(TrainerApiService);
  private readonly router = inject(Router);

  // signal(), não campos soltos — ver enrollments-list.ts/app.config.ts.
  readonly errorMessage = signal<string | null>(null);
  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, emailWithTldValidator]],
    city: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.submitting.set(true);

    this.trainerApi.create(this.form.getRawValue()).subscribe({
      // FR-023: confirmação de sucesso + redirecionamento para a listagem.
      next: () => {
        this.submitting.set(false);
        this.router.navigate(['/treinadores'], { state: { successMessage: 'Treinador cadastrado com sucesso.' } });
      },
      error: (err: Error) => {
        this.submitting.set(false);
        this.errorMessage.set(err.message);
      },
    });
  }
}
