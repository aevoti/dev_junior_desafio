export interface Pokemon {
  id: number;
  nome: string;
  tipo: string;
  nivel: number;
  treinadorId: number;
  treinadorNome: string;
}

/** R5: transfere o Pokémon para outro Treinador. */
export interface TransferirPokemonRequest {
  novoTreinadorId: number;
}
