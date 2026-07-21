import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api-config';
import { Pokemon } from '../../shared/models/pokemon.model';

@Injectable({ providedIn: 'root' })
export class PokemonService {
  private readonly baseUrl = `${API_BASE_URL}/pokemons`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Pokemon[]> {
    return this.http.get<Pokemon[]>(this.baseUrl);
  }

  getById(id: number): Observable<Pokemon> {
    return this.http.get<Pokemon>(`${this.baseUrl}/${id}`);
  }

  transferir(pokemonId: number, novoTreinadorId: number): Observable<void> {
  return this.http.patch<void>(`${this.baseUrl}/${pokemonId}/transferir`, {
    novoTreinadorId
  });
}
}