import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../config/api.config';
import { Plano } from '../models/plano.model';

@Injectable({
  providedIn: 'root',
})
export class PlanoService {
  private readonly http = inject(HttpClient);

  listar(): Observable<Plano[]> {
    return this.http.get<Plano[]>(`${API_BASE_URL}/planos`);
  }
}
