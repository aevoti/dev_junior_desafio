import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { PokemonRequest } from '../../models/pokemon/pokemon-request';
import { PokemonResponse } from '../../models/pokemon/pokemon-response';
import { API_URL } from '../api.config';

@Injectable({
  providedIn: 'root'
})
export class PokemonService {
  private readonly apiUrl = `${API_URL}/pokemons`;

  constructor(
    private readonly httpClient: HttpClient
  ) {}

  listar(busca?: string): Observable<PokemonResponse[]> {
    let parametros: HttpParams = new HttpParams();

    if (busca?.trim()) {
      parametros = parametros.set('busca', busca.trim());
    }

    return this.httpClient.get<PokemonResponse[]>(
      this.apiUrl,
      {
        params: parametros
      }
    );
  }

  recuperarPorId(id: number): Observable<PokemonResponse> {
    return this.httpClient.get<PokemonResponse>(
      `${this.apiUrl}/${id}`
    );
  }

  listarPorTreinador(
    treinadorId: number
  ): Observable<PokemonResponse[]> {
    return this.httpClient.get<PokemonResponse[]>(
      `${this.apiUrl}/treinador/${treinadorId}`
    );
  }

  inserir(
    request: PokemonRequest
  ): Observable<PokemonResponse> {
    return this.httpClient.post<PokemonResponse>(
      this.apiUrl,
      request
    );
  }
}