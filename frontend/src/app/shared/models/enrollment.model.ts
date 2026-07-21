export type EnrollmentStatus = 'Active' | 'EndingSoon' | 'Ended';

/** English API status code → Portuguese label shown to the user (Princípio II). */
export const ENROLLMENT_STATUS_LABELS: Record<EnrollmentStatus, string> = {
  Active: 'Ativa',
  EndingSoon: 'A encerrar',
  Ended: 'Encerrada',
};

export interface Enrollment {
  id: number;
  pokemonName: string;
  trainerName: string;
  trainingPlanName: string;
  startDate: string;
  endDate: string | null;
  monthlyPrice: number;
  status: EnrollmentStatus;
}

export interface CreateEnrollmentRequest {
  pokemonId: number;
  trainingPlanId: number;
}

export interface UpgradeRequest {
  newTrainingPlanId: number;
}

export interface UpgradePreviewResponse {
  currentPlanCredit: number;
  newPlanProratedCost: number;
  firstChargeAmount: number;
  cycleEndDate: string;
  daysRemainingInCycle: number;
}

export interface UpgradeConfirmResponse {
  closedEnrollmentId: number;
  newEnrollment: Enrollment & { id: number };
  firstChargeAmount: number;
}
