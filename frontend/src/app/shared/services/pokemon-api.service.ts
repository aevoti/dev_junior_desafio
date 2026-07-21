import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../api-base-url';
import {
  CreatePokemonRequest,
  Pokemon,
  TransferPokemonRequest,
  TransferPokemonResponse,
} from '../models/pokemon.model';

@Injectable({ providedIn: 'root' })
export class PokemonApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/pokemons`;

  list(): Observable<Pokemon[]> {
    return this.http.get<Pokemon[]>(this.baseUrl);
  }

  create(request: CreatePokemonRequest): Observable<Pokemon> {
    return this.http.post<Pokemon>(this.baseUrl, request);
  }

  transfer(pokemonId: number, request: TransferPokemonRequest): Observable<TransferPokemonResponse> {
    return this.http.post<TransferPokemonResponse>(`${this.baseUrl}/${pokemonId}/transfer`, request);
  }
}
