import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PlanoTreinamento } from '../models/plano-treinamento.model';

@Injectable({ providedIn: 'root' })
export class PlanoTreinamentoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/planos-treinamento`;

  listar(): Observable<PlanoTreinamento[]> {
    return this.http.get<PlanoTreinamento[]>(this.baseUrl);
  }
}
