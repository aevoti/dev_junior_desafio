import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../api-base-url';
import {
  CreateEnrollmentRequest,
  Enrollment,
  EnrollmentStatus,
  UpgradeConfirmResponse,
  UpgradePreviewResponse,
  UpgradeRequest,
} from '../models/enrollment.model';

@Injectable({ providedIn: 'root' })
export class EnrollmentApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/enrollments`;

  list(search?: string, status?: EnrollmentStatus): Observable<Enrollment[]> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<Enrollment[]>(this.baseUrl, { params });
  }

  create(request: CreateEnrollmentRequest): Observable<Enrollment> {
    return this.http.post<Enrollment>(this.baseUrl, request);
  }

  previewUpgrade(enrollmentId: number, request: UpgradeRequest): Observable<UpgradePreviewResponse> {
    return this.http.post<UpgradePreviewResponse>(`${this.baseUrl}/${enrollmentId}/upgrade/preview`, request);
  }

  confirmUpgrade(enrollmentId: number, request: UpgradeRequest): Observable<UpgradeConfirmResponse> {
    return this.http.post<UpgradeConfirmResponse>(`${this.baseUrl}/${enrollmentId}/upgrade/confirm`, request);
  }

  cancel(enrollmentId: number): Observable<Enrollment> {
    return this.http.post<Enrollment>(`${this.baseUrl}/${enrollmentId}/cancel`, {});
  }
}
