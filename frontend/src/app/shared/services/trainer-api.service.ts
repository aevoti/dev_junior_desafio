import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../api-base-url';
import { CreateTrainerRequest, Trainer } from '../models/trainer.model';

@Injectable({ providedIn: 'root' })
export class TrainerApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/trainers`;

  list(): Observable<Trainer[]> {
    return this.http.get<Trainer[]>(this.baseUrl);
  }

  create(request: CreateTrainerRequest): Observable<Trainer> {
    return this.http.post<Trainer>(this.baseUrl, request);
  }
}
