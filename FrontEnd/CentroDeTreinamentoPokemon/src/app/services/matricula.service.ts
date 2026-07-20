import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import {
  Matricula,
  NovaMatriculaPayload,
  UpgradePayload,
  UpgradeSimulacao,
} from '../models/matricula.model';

@Injectable({
  providedIn: 'root',
})
export class MatriculaService {
  private readonly http = inject(HttpClient);

  listar(): Observable<Matricula[]> {
    return this.http.get<Matricula[]>(`${API_BASE_URL}/matriculas`);
  }

  obterPorId(id: number): Observable<Matricula> {
    return this.http.get<Matricula>(`${API_BASE_URL}/matriculas/${id}`);
  }

  criar(payload: NovaMatriculaPayload): Observable<Matricula> {
    return this.http.post<Matricula>(`${API_BASE_URL}/matriculas`, payload);
  }

  cancelar(id: number): Observable<Matricula> {
    return this.http.patch<Matricula>(`${API_BASE_URL}/matriculas/${id}/cancelar`, null);
  }

  simularUpgrade(matriculaId: number, novoPlanoId: number): Observable<UpgradeSimulacao> {
    return this.http.get<UpgradeSimulacao>(
      `${API_BASE_URL}/matriculas/${matriculaId}/upgrade?novoPlanoId=${novoPlanoId}`,
    );
  }

  confirmarUpgrade(matriculaId: number, payload: UpgradePayload): Observable<Matricula> {
    return this.http.patch<Matricula>(`${API_BASE_URL}/matriculas/${matriculaId}/upgrade`, payload);
  }
}
