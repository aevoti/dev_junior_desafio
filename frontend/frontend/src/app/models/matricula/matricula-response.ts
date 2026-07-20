export interface MatriculaResponse {
  id: number;
  pokemonId: number;
  pokemonNome: string;
  treinadorId: number;
  treinadorNome: string;
  planoTreinamentoId: number;
  planoTreinamentoNome: string;
  dataInicio: string;
  dataEncerramento: string | null;
  status: number;
  valorMensal: number;
}