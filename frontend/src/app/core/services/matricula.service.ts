import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CriarMatriculaRequest,
  Matricula,
  SimulacaoUpgrade,
  StatusMatricula,
  UpgradeMatriculaRequest,
  UpgradeMatriculaResponse
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class MatriculaService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/matriculas`;

  listar(busca?: string, status?: StatusMatricula | ''): Observable<Matricula[]> {
    let params = new HttpParams();
    if (busca?.trim()) params = params.set('busca', busca.trim());
    if (status) params = params.set('status', status);
    return this.http.get<Matricula[]>(this.url, { params });
  }

  criar(request: CriarMatriculaRequest): Observable<Matricula> {
    return this.http.post<Matricula>(this.url, request);
  }

  simularUpgrade(matriculaId: number, request: UpgradeMatriculaRequest): Observable<SimulacaoUpgrade> {
    return this.http.post<SimulacaoUpgrade>(`${this.url}/${matriculaId}/simular-upgrade`, request);
  }

  realizarUpgrade(matriculaId: number, request: UpgradeMatriculaRequest): Observable<UpgradeMatriculaResponse> {
    return this.http.post<UpgradeMatriculaResponse>(`${this.url}/${matriculaId}/upgrade`, request);
  }
}
