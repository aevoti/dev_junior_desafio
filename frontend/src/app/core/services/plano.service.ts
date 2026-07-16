import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PlanoTreinamento } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class PlanoService {
  private readonly http = inject(HttpClient);

  listar(): Observable<PlanoTreinamento[]> {
    return this.http.get<PlanoTreinamento[]>(`${environment.apiUrl}/planos`);
  }
}
