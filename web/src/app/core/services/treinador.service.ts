import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Treinador } from '../models/treinador.model';

@Injectable({ providedIn: 'root' })
export class TreinadorService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/treinadores`;

  listar(): Observable<Treinador[]> {
    return this.http.get<Treinador[]>(this.baseUrl);
  }
}
