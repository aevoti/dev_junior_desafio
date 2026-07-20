import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { Treinador, TreinadorPayload } from '../models/treinador.model';

@Injectable({
  providedIn: 'root',
})
export class TreinadorService {
  private readonly http = inject(HttpClient);

  listar(): Observable<Treinador[]> {
    return this.http.get<Treinador[]>(`${API_BASE_URL}/treinadores`);
  }

  obterPorId(id: number): Observable<Treinador> {
    return this.http.get<Treinador>(`${API_BASE_URL}/treinadores/${id}`);
  }

  criar(payload: TreinadorPayload): Observable<Treinador> {
    return this.http.post<Treinador>(`${API_BASE_URL}/treinadores`, payload);
  }
}
