import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../api-base-url';
import { TrainingPlan } from '../models/training-plan.model';

@Injectable({ providedIn: 'root' })
export class TrainingPlanApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/training-plans`;

  list(): Observable<TrainingPlan[]> {
    return this.http.get<TrainingPlan[]>(this.baseUrl);
  }
}
