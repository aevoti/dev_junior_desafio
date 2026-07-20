export interface PokeApiPokemonResponse {
  id: number;
  name: string;
  sprites: {
    front_default: string | null;
    other: {
      'official-artwork': {
        front_default: string | null;
      };
    };
  };
  types: Array<{
    type: {
      name: string;
    };
  }>;
}

export interface PokemonVisual {
  id: number;
  nome: string;
  imagem: string | null;
  tipos: string[];
}
