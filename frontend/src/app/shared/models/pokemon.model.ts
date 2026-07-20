export interface Pokemon {
  id: number;
  name: string;
  type: string;
  level: number;
  trainerId: number;
  trainerName: string;
}

export interface CreatePokemonRequest {
  name: string;
  type: string;
  level: number;
  trainerId: number;
}

export interface TransferPokemonRequest {
  newTrainerId: number;
}

export interface TransferPokemonResponse {
  pokemon: Pokemon;
  closedEnrollmentId: number | null;
  newEnrollmentId: number | null;
}

/** Fixed list of the 18 official Pokémon types (FR-022) — English code, Portuguese label (Princípio II). */
export const POKEMON_TYPES: ReadonlyArray<{ value: string; label: string }> = [
  { value: 'Normal', label: 'Normal' },
  { value: 'Fire', label: 'Fogo' },
  { value: 'Water', label: 'Água' },
  { value: 'Grass', label: 'Planta' },
  { value: 'Electric', label: 'Elétrico' },
  { value: 'Ice', label: 'Gelo' },
  { value: 'Fighting', label: 'Lutador' },
  { value: 'Poison', label: 'Venenoso' },
  { value: 'Ground', label: 'Terra' },
  { value: 'Flying', label: 'Voador' },
  { value: 'Psychic', label: 'Psíquico' },
  { value: 'Bug', label: 'Inseto' },
  { value: 'Rock', label: 'Pedra' },
  { value: 'Ghost', label: 'Fantasma' },
  { value: 'Dragon', label: 'Dragão' },
  { value: 'Dark', label: 'Sombrio' },
  { value: 'Steel', label: 'Aço' },
  { value: 'Fairy', label: 'Fada' },
];
