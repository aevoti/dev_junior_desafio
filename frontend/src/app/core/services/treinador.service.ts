import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Treinador } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class TreinadorService {
  private readonly http = inject(HttpClient);

  listar(): Observable<Treinador[]> {
    return this.http.get<Treinador[]>(`${environment.apiUrl}/treinadores`);
  }
}
