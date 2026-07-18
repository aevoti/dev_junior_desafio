export interface Matricula {
  id: number;
  pokemonId: number;
  nomePokemon: string;
  nomeTreinador: string;
  plano: string;
  dataInicio: string;
  status: string;
  valorMensal: number;
}

export interface Pokemon {
  id: number;
  nome: string;
  tipo: string;
  nivel: number;
  treinadorId: number;
  nomeTreinador: string;
}

export interface Treinador {
  id: number;
  nome: string;
  email: string;
  cidadeOrigem: string;
}

export interface UpgradePreview {
  primeiraCobranca: number;
  creditoPlanoAntigo: number;
  custoNovoPlanoDiasRestantes: number;
  diasRestantes: number;
  descricao: string;
}

export const PLANOS = [
  { valor: 'GinasioLocal',  label: 'Ginásio Local',  preco: 50 },
  { valor: 'LigaRegional',  label: 'Liga Regional',   preco: 120 },
  { valor: 'EliteDos4',     label: 'Elite dos 4',     preco: 300 }
];

export const STATUS_LIST = ['Ativa', 'Cancelada', 'Concluida'];
