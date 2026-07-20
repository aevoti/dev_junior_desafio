export interface Matricula {
  id: number;
  dataInicio: string;
  dataFim: string | null;
  pokemonId: number;
  pokemonNome: string;
  nivel: number;
  treinadorId: number;
  treinadorNome: string;
  planoId: number;
  planoNome: string;
  planoValorMensal: number;
  valorPrimeiraCobranca: number;
  status: 'Ativa' | 'Cancelada' | 'Concluida';
}

export interface NovaMatriculaPayload {
  pokemonId: number;
  planoId: number;
}

export interface UpgradePayload {
  novoPlanoId: number;
}

export interface UpgradeSimulacao {
  primeiraCobranca: number;
}
