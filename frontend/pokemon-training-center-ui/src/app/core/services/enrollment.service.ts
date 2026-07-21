import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './api-config';
import {
  Enrollment,
  CreateEnrollmentRequest,
  UpgradeRequest,
  UpgradeSimulationResult
} from '../../shared/models/enrollment.model';
import { EnrollmentStatus } from '../../shared/models/enrollment-status.model';

@Injectable({ providedIn: 'root' })
export class EnrollmentService {
  private readonly baseUrl = `${API_BASE_URL}/matriculas`;

  constructor(private http: HttpClient) {}

  getAll(busca?: string, status?: EnrollmentStatus): Observable<Enrollment[]> {
    let params = new HttpParams();
    if (busca) params = params.set('busca', busca);
    if (status) params = params.set('status', status);
    return this.http.get<Enrollment[]>(this.baseUrl, { params });
  }

  create(request: CreateEnrollmentRequest): Observable<Enrollment> {
    return this.http.post<Enrollment>(this.baseUrl, request);
  }

  simulateUpgrade(id: number, request: UpgradeRequest): Observable<UpgradeSimulationResult> {
    return this.http.post<UpgradeSimulationResult>(`${this.baseUrl}/${id}/upgrade/simular`, request);
  }

  confirmUpgrade(id: number, request: UpgradeRequest): Observable<Enrollment> {
    return this.http.post<Enrollment>(`${this.baseUrl}/${id}/upgrade/confirmar`, request);
  }

  cancel(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/cancelar`, {});
  }
}