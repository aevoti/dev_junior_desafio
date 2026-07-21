import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api-config';
import { Trainer } from '../../shared/models/trainer.model';

@Injectable({ providedIn: 'root' })
export class TrainerService {
  private readonly baseUrl = `${API_BASE_URL}/treinadores`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Trainer[]> {
    return this.http.get<Trainer[]>(this.baseUrl);
  }
}