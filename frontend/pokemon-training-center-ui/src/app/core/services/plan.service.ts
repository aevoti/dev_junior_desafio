import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api-config';
import { Plan } from '../../shared/models/plan.model';

@Injectable({ providedIn: 'root' })
export class PlanService {
  private readonly baseUrl = `${API_BASE_URL}/planos`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Plan[]> {
    return this.http.get<Plan[]>(this.baseUrl);
  }
}