import { PokemonType } from './pokemon-type.model';

export interface Pokemon {
  id: number;
  nome: string;
  tipo: PokemonType;
  nivel: number;
  treinadorId: number;
  treinadorNome: string;
}