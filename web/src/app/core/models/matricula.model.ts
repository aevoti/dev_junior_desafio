export type StatusMatricula = 'Ativa' | 'Cancelada' | 'Concluida';

export interface Matricula {
  id: number;
  pokemonId: number;
  pokemonNome: string;
  treinadorNome: string;
  planoTreinamentoId: number;
  planoTreinamentoNome: string;
  dataInicio: string;
  dataFim: string | null;
  status: StatusMatricula;
  valorMensal: number;
}

export interface CriarMatriculaRequest {
  pokemonId: number;
  planoTreinamentoId: number;
  dataInicio: string;
}

export interface UpgradeMatriculaRequest {
  novoPlanoTreinamentoId: number;
  dataUpgrade: string;
}

export interface UpgradeMatriculaResponse {
  novaMatriculaId: number;
  creditoPlanoAntigo: number;
  custoNovoPlanoRestante: number;
  valorPrimeiraCobranca: number;
}
