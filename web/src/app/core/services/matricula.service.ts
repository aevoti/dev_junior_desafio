import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CriarMatriculaRequest,
  Matricula,
  StatusMatricula,
  UpgradeMatriculaRequest,
  UpgradeMatriculaResponse,
} from '../models/matricula.model';

@Injectable({ providedIn: 'root' })
export class MatriculaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/matriculas`;

  listar(busca?: string, status?: StatusMatricula): Observable<Matricula[]> {
    let params = new HttpParams();
    if (busca) params = params.set('busca', busca);
    if (status) params = params.set('status', status);
    return this.http.get<Matricula[]>(this.baseUrl, { params });
  }

  criar(request: CriarMatriculaRequest): Observable<Matricula> {
    return this.http.post<Matricula>(this.baseUrl, request);
  }

  // R2: calcula o valor da primeira cobrança pro-rata SEM efetivar o upgrade — usado
  // para exibir o valor ao usuário antes da confirmação.
  simularUpgrade(matriculaId: number, request: UpgradeMatriculaRequest): Observable<UpgradeMatriculaResponse> {
    return this.http.post<UpgradeMatriculaResponse>(`${this.baseUrl}/${matriculaId}/upgrade/simular`, request);
  }

  // R2: efetiva o upgrade (encerra a matrícula atual e cria a nova no plano superior).
  upgrade(matriculaId: number, request: UpgradeMatriculaRequest): Observable<UpgradeMatriculaResponse> {
    return this.http.post<UpgradeMatriculaResponse>(`${this.baseUrl}/${matriculaId}/upgrade`, request);
  }

  cancelar(matriculaId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${matriculaId}/cancelar`, {});
  }
}
