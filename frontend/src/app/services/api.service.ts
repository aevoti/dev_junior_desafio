import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Matricula, Pokemon, Treinador, UpgradePreview } from '../models/matricula';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private base = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  // Matrículas
  getMatriculas(busca?: string, status?: string): Observable<Matricula[]> {
    let params = new HttpParams();
    if (busca) params = params.set('busca', busca);
    if (status) params = params.set('status', status);
    return this.http.get<Matricula[]>(`${this.base}/matriculas`, { params });
  }

  criarMatricula(pokemonId: number, plano: string): Observable<Matricula> {
    return this.http.post<Matricula>(`${this.base}/matriculas`, { pokemonId, plano });
  }

  cancelarMatricula(id: number): Observable<void> {
    return this.http.patch<void>(`${this.base}/matriculas/${id}/cancelar`, {});
  }

  previewUpgrade(id: number, novoPlano: string): Observable<UpgradePreview> {
    const params = new HttpParams().set('novoPlano', novoPlano);
    return this.http.get<UpgradePreview>(`${this.base}/matriculas/${id}/upgrade/preview`, { params });
  }

  executarUpgrade(id: number, novoPlano: string): Observable<Matricula> {
    return this.http.post<Matricula>(`${this.base}/matriculas/${id}/upgrade`, { novoPlano });
  }

  // Pokémons
  getPokemons(): Observable<Pokemon[]> {
    return this.http.get<Pokemon[]>(`${this.base}/pokemons`);
  }

  // Treinadores
  getTreinadores(): Observable<Treinador[]> {
    return this.http.get<Treinador[]>(`${this.base}/treinadores`);
  }
}
