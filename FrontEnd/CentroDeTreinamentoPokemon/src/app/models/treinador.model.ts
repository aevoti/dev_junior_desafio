export interface Treinador {
  id: number;
  nome: string;
  email: string;
  cidadeDeOrigem: string;
}
export interface TreinadorPayload {
  nome: string;
  email: string;
  cidadeDeOrigem: string;
}
