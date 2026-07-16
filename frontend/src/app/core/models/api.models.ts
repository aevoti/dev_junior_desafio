export interface Treinador {
  id: number;
  nome: string;
  email: string;
  cidadeOrigem: string;
}

export interface Pokemon {
  id: number;
  nome: string;
  tipo: string;
  nivel: number;
  treinadorId: number;
  treinadorNome: string;
}

export interface PlanoTreinamento {
  id: number;
  nome: string;
  valorMensal: number;
  descricao: string;
  ordem: number;
  nivelMinimo: number;
}

export type StatusMatricula = 'Ativa' | 'Cancelada' | 'Concluida';

export interface Matricula {
  id: number;
  pokemonId: number;
  pokemonNome: string;
  treinadorId: number;
  treinadorNome: string;
  planoTreinamentoId: number;
  planoNome: string;
  dataInicio: string;
  dataFim: string | null;
  status: StatusMatricula;
  valorMensal: number;
  motivoEncerramento: string | null;
}

export interface CriarMatriculaRequest {
  pokemonId: number;
  planoTreinamentoId: number;
  dataInicio: string;
}

export interface UpgradeMatriculaRequest {
  novoPlanoTreinamentoId: number;
}

export interface SimulacaoUpgrade {
  matriculaAtualId: number;
  planoAtualId: number;
  planoAtualNome: string;
  novoPlanoId: number;
  novoPlanoNome: string;
  diasRestantes: number;
  creditoPlanoAnterior: number;
  custoProporcionalNovoPlano: number;
  primeiraCobranca: number;
  dataUpgrade: string;
}

export interface UpgradeMatriculaResponse {
  matriculaAnteriorId: number;
  novaMatriculaId: number;
  pokemonId: number;
  pokemonNome: string;
  planoAnteriorId: number;
  planoAnteriorNome: string;
  novoPlanoId: number;
  novoPlanoNome: string;
  diasRestantes: number;
  creditoPlanoAnterior: number;
  custoProporcionalNovoPlano: number;
  primeiraCobranca: number;
  dataUpgrade: string;
}
