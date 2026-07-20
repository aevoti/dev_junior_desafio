import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { UpgradePreviewResponse } from '../shared/models/enrollment.model';
import { TrainingPlan } from '../shared/models/training-plan.model';
import { EnrollmentApiService } from '../shared/services/enrollment-api.service';
import { TrainingPlanApiService } from '../shared/services/training-plan-api.service';

@Component({
  selector: 'app-enrollment-upgrade',
  imports: [ReactiveFormsModule],
  templateUrl: './enrollment-upgrade.html',
  styleUrl: './enrollment-upgrade.css',
})
export class EnrollmentUpgrade implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly enrollmentApi = inject(EnrollmentApiService);
  private readonly trainingPlanApi = inject(TrainingPlanApiService);

  private enrollmentId!: number;
  trainingPlans: TrainingPlan[] = [];
  errorMessage: string | null = null;
  submitting = false;

  /** Preenchido após "Calcular" (US2, cenário 1) — só confirma depois de ver o valor. */
  preview: UpgradePreviewResponse | null = null;

  readonly form = this.fb.nonNullable.group({
    newTrainingPlanId: [null as number | null, Validators.required],
  });

  ngOnInit(): void {
    this.enrollmentId = Number(this.route.snapshot.paramMap.get('id'));
    this.trainingPlanApi.list().subscribe((plans) => (this.trainingPlans = plans));

    this.form.controls.newTrainingPlanId.valueChanges.subscribe(() => (this.preview = null));
  }

  calculatePreview(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage = null;
    this.preview = null;
    this.submitting = true;

    this.enrollmentApi
      .previewUpgrade(this.enrollmentId, { newTrainingPlanId: this.form.getRawValue().newTrainingPlanId! })
      .subscribe({
        next: (preview) => {
          this.submitting = false;
          this.preview = preview;
        },
        // R2 (downgrade) e R3 (nível mínimo) chegam aqui como mensagem amigável.
        error: (err: Error) => {
          this.submitting = false;
          this.errorMessage = err.message;
        },
      });
  }

  confirm(): void {
    if (!this.preview) {
      return;
    }

    this.errorMessage = null;
    this.submitting = true;

    this.enrollmentApi
      .confirmUpgrade(this.enrollmentId, { newTrainingPlanId: this.form.getRawValue().newTrainingPlanId! })
      .subscribe({
        next: () => {
          this.submitting = false;
          this.router.navigateByUrl('/matriculas');
        },
        error: (err: Error) => {
          this.submitting = false;
          this.errorMessage = err.message;
        },
      });
  }
}
