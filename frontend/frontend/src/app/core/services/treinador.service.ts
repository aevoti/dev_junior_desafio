import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { TreinadorRequest } from '../../models/treinador/treinador-request';
import { TreinadorResponse } from '../../models/treinador/treinador-response';
import { API_URL } from '../api.config';

@Injectable({
  providedIn: 'root'
})
export class TreinadorService {
  private readonly apiUrl = `${API_URL}/treinadores`;

  constructor(
    private readonly httpClient: HttpClient
  ) {}

  listar(busca?: string): Observable<TreinadorResponse[]> {
    let parametros: HttpParams = new HttpParams();

    if (busca?.trim()) {
      parametros = parametros.set('busca', busca.trim());
    }

    return this.httpClient.get<TreinadorResponse[]>(
      this.apiUrl,
      {
        params: parametros
      }
    );
  }

  recuperarPorId(id: number): Observable<TreinadorResponse> {
    return this.httpClient.get<TreinadorResponse>(
      `${this.apiUrl}/${id}`
    );
  }

  inserir(request: TreinadorRequest): Observable<TreinadorResponse> {
    return this.httpClient.post<TreinadorResponse>(
      this.apiUrl,
      request
    );
  }
}