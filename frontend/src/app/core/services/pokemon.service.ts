import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Pokemon } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class PokemonService {
  private readonly http = inject(HttpClient);

  listar(): Observable<Pokemon[]> {
    return this.http.get<Pokemon[]>(`${environment.apiUrl}/pokemons`);
  }
}
