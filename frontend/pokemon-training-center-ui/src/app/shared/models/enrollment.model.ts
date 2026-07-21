import { EnrollmentStatus } from './enrollment-status.model';

export interface Enrollment {
  id: number;
  pokemonNome: string;
  treinadorNome: string;
  planoNome: string;
  valorMensal: number;
  dataInicio: string;
  status: EnrollmentStatus;
}

export interface CreateEnrollmentRequest {
  pokemonId: number;
  planoId: number;
  dataInicio: string;
}

export interface UpgradeRequest {
  novoPlanoId: number;
  dataUpgrade: string;
}

export interface UpgradeSimulationResult {
  valorPrimeiraCobranca: number;
  diasRestantes: number;
  creditoPlanoAntigo: number;
  custoNovoPlano: number;
}