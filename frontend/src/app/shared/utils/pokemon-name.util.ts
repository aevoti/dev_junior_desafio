export interface PokemonNameParts {
  base: string;
  complement: string;
}

export function splitPokemonName(fullName: string): PokemonNameParts {
  const normalized = fullName.trim();
  const firstSpace = normalized.search(/\s/);

  if (firstSpace < 0) {
    return { base: normalized, complement: '' };
  }

  return {
    base: normalized.slice(0, firstSpace),
    complement: normalized.slice(firstSpace).trim()
  };
}
