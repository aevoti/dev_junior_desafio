import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Pokemon, PokemonPayload, TransferirPokemonPayload } from '../models/pokemon.model';
import { API_BASE_URL } from '../config/api.config';

@Injectable({
  providedIn: 'root',
})
export class PokemonService {
  private readonly http = inject(HttpClient);

  listar(): Observable<Pokemon[]> {
    return this.http.get<Pokemon[]>(`${API_BASE_URL}/pokemons`);
  }

  obterPorId(id: number): Observable<Pokemon> {
    return this.http.get<Pokemon>(`${API_BASE_URL}/pokemons/${id}`);
  }

  criar(payload: PokemonPayload): Observable<Pokemon> {
    return this.http.post<Pokemon>(`${API_BASE_URL}/pokemons`, payload);
  }

  transferir(id: number, payload: TransferirPokemonPayload): Observable<Pokemon> {
    return this.http.patch<Pokemon>(`${API_BASE_URL}/pokemons/${id}/transferir`, payload);
  }
}
