import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, of, shareReplay } from 'rxjs';
import { PokeApiPokemonResponse, PokemonVisual } from '../models/poke-api.models';

@Injectable({ providedIn: 'root' })
export class PokeApiService {
  private readonly http = inject(HttpClient);
  private readonly cache = new Map<string, Observable<PokemonVisual | null>>();

  buscar(nomeRegistrado: string): Observable<PokemonVisual | null> {
    const nome = this.normalizarNome(nomeRegistrado);
    if (!nome) return of(null);

    const existente = this.cache.get(nome);
    if (existente) return existente;

    const requisicao = this.http
      .get<PokeApiPokemonResponse>(`https://pokeapi.co/api/v2/pokemon/${encodeURIComponent(nome)}`)
      .pipe(
        map(pokemon => ({
          id: pokemon.id,
          nome: pokemon.name,
          imagem: pokemon.sprites.other['official-artwork'].front_default ?? pokemon.sprites.front_default,
          tipos: pokemon.types.map(item => item.type.name)
        })),
        catchError(() => of(null)),
        shareReplay({ bufferSize: 1, refCount: false })
      );

    this.cache.set(nome, requisicao);
    return requisicao;
  }

  normalizarNome(nomeRegistrado: string): string {
    return nomeRegistrado
      .trim()
      .split(/\s+/)[0]
      .toLocaleLowerCase('pt-BR')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9-]/g, '');
  }
}
