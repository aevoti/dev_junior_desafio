import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { PlanoTreinamentoResponse } from '../../models/plano-treinamento/plano-treinamento-response';
import { API_URL } from '../api.config';

@Injectable({
  providedIn: 'root'
})
export class PlanoTreinamentoService {
  private readonly apiUrl = `${API_URL}/planos`;

  constructor(
    private readonly httpClient: HttpClient
  ) {}

  listar(): Observable<PlanoTreinamentoResponse[]> {
    return this.httpClient.get<PlanoTreinamentoResponse[]>(
      this.apiUrl
    );
  }

  recuperarPorId(
    id: number
  ): Observable<PlanoTreinamentoResponse> {
    return this.httpClient.get<PlanoTreinamentoResponse>(
      `${this.apiUrl}/${id}`
    );
  }
}