export interface Trainer {
  id: number;
  name: string;
  email: string;
  city: string;
}

export interface CreateTrainerRequest {
  name: string;
  email: string;
  city: string;
}
