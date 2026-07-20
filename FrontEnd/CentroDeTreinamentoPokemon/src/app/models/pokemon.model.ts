export interface Pokemon {
  id: number;
  nome: string;
  tipo: string;
  nivel: number;
  treinadorId: number;
}

export interface PokemonPayload {
  nome: string;
  tipo: string;
  nivel: number;
  treinadorId: number;
}

export interface TransferirPokemonPayload {
  novoTreinadorId: number;
}
