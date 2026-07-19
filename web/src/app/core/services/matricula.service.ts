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

  // R2: retorna o valor calculado da primeira cobrança pro-rata, para confirmação do usuário antes de efetivar.
  upgrade(matriculaId: number, request: UpgradeMatriculaRequest): Observable<UpgradeMatriculaResponse> {
    return this.http.post<UpgradeMatriculaResponse>(`${this.baseUrl}/${matriculaId}/upgrade`, request);
  }

  cancelar(matriculaId: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${matriculaId}/cancelar`, {});
  }
}
