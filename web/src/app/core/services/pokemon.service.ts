import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Pokemon, TransferirPokemonRequest } from '../models/pokemon.model';

@Injectable({ providedIn: 'root' })
export class PokemonService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/pokemons`;

  listar(): Observable<Pokemon[]> {
    return this.http.get<Pokemon[]>(this.baseUrl);
  }

  // R5: matrículas continuam ligadas ao Pokémon (não ao Treinador), então o
  // histórico acompanha a transferência automaticamente — nada mais a fazer aqui.
  transferir(pokemonId: number, request: TransferirPokemonRequest): Observable<Pokemon> {
    return this.http.put<Pokemon>(`${this.baseUrl}/${pokemonId}/transferir`, request);
  }
}
