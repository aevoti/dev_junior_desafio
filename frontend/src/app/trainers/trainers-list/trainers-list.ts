import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { Trainer } from '../../shared/models/trainer.model';
import { TrainerApiService } from '../../shared/services/trainer-api.service';

@Component({
  selector: 'app-trainers-list',
  imports: [RouterLink],
  templateUrl: './trainers-list.html',
  styleUrl: './trainers-list.css',
})
export class TrainersList implements OnInit {
  private readonly trainerApi = inject(TrainerApiService);
  private readonly router = inject(Router);

  readonly trainers = signal<Trainer[]>([]);
  readonly loading = signal(false);
  // FR-023: mensagem de sucesso vinda do redirecionamento pós-cadastro (router state).
  readonly successMessage = signal<string | null>(
    (this.router.getCurrentNavigation()?.extras.state as { successMessage?: string } | undefined)?.successMessage ?? null
  );

  ngOnInit(): void {
    this.loading.set(true);
    this.trainerApi.list().subscribe((trainers) => {
      this.trainers.set(trainers);
      this.loading.set(false);
    });
  }
}
