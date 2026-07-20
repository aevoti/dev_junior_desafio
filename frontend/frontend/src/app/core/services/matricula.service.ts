import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { MatriculaRequest } from '../../models/matricula/matricula-request';
import { MatriculaResponse } from '../../models/matricula/matricula-response';
import { UpgradeMatriculaRequest } from '../../models/matricula/upgrade-matricula-request';
import { UpgradeMatriculaResponse } from '../../models/matricula/upgrade-matricula-response';
import { API_URL } from '../api.config';

@Injectable({
  providedIn: 'root'
})
export class MatriculaService {
  private readonly apiUrl = `${API_URL}/matriculas`;

  constructor(
    private readonly httpClient: HttpClient
  ) {}

  listar(
    busca?: string,
    status?: number
  ): Observable<MatriculaResponse[]> {
    let parametros = new HttpParams();

    if (busca?.trim()) {
      parametros = parametros.set('busca', busca.trim());
    }

    if (status !== undefined) {
      parametros = parametros.set('status', status.toString());
    }

    return this.httpClient.get<MatriculaResponse[]>(
      this.apiUrl,
      {
        params: parametros
      }
    );
  }

  recuperarPorId(id: number): Observable<MatriculaResponse> {
    return this.httpClient.get<MatriculaResponse>(
      `${this.apiUrl}/${id}`
    );
  }

  inserir(request: MatriculaRequest): Observable<MatriculaResponse> {
    return this.httpClient.post<MatriculaResponse>(
      this.apiUrl,
      request
    );
  }

  cancelar(id: number): Observable<MatriculaResponse> {
    return this.httpClient.post<MatriculaResponse>(
      `${this.apiUrl}/${id}/cancelar`,
      {}
    );
  }

  simularUpgrade(
    id: number,
    request: UpgradeMatriculaRequest
  ): Observable<UpgradeMatriculaResponse> {
    return this.httpClient.post<UpgradeMatriculaResponse>(
      `${this.apiUrl}/${id}/upgrade/simular`,
      request
    );
  }

  confirmarUpgrade(
    id: number,
    request: UpgradeMatriculaRequest
  ): Observable<UpgradeMatriculaResponse> {
    return this.httpClient.post<UpgradeMatriculaResponse>(
      `${this.apiUrl}/${id}/upgrade/confirmar`,
      request
    );
  }
  
}