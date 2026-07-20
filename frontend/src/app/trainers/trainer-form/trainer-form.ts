import { Component, inject, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { Trainer } from '../../shared/models/trainer.model';
import { TrainerApiService } from '../../shared/services/trainer-api.service';

@Component({
  selector: 'app-trainer-form',
  imports: [ReactiveFormsModule],
  templateUrl: './trainer-form.html',
  styleUrl: './trainer-form.css',
})
export class TrainerForm {
  private readonly fb = inject(FormBuilder);
  private readonly trainerApi = inject(TrainerApiService);

  readonly created = output<Trainer>();

  errorMessage: string | null = null;
  submitting = false;

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    city: ['', Validators.required],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage = null;
    this.submitting = true;

    this.trainerApi.create(this.form.getRawValue()).subscribe({
      next: (trainer) => {
        this.submitting = false;
        this.form.reset();
        this.created.emit(trainer);
      },
      error: (err: Error) => {
        this.submitting = false;
        this.errorMessage = err.message;
      },
    });
  }
}
