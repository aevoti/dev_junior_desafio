import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { POKEMON_TYPES, Pokemon } from '../../shared/models/pokemon.model';
import { PokemonApiService } from '../../shared/services/pokemon-api.service';

@Component({
  selector: 'app-pokemons-list',
  imports: [RouterLink],
  templateUrl: './pokemons-list.html',
  styleUrl: './pokemons-list.css',
})
export class PokemonsList implements OnInit {
  private readonly pokemonApi = inject(PokemonApiService);
  private readonly router = inject(Router);

  private readonly typeLabels = new Map(POKEMON_TYPES.map((type) => [type.value, type.label]));

  readonly pokemons = signal<Pokemon[]>([]);
  readonly loading = signal(false);
  // FR-023: mensagem de sucesso vinda do redirecionamento pós-cadastro (router state).
  readonly successMessage = signal<string | null>(
    (this.router.getCurrentNavigation()?.extras.state as { successMessage?: string } | undefined)?.successMessage ?? null
  );

  ngOnInit(): void {
    this.loading.set(true);
    this.pokemonApi.list().subscribe((pokemons) => {
      this.pokemons.set(pokemons);
      this.loading.set(false);
    });
  }

  typeLabel(type: string): string {
    return this.typeLabels.get(type) ?? type;
  }
}
