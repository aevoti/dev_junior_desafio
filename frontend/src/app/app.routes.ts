import { Routes } from '@angular/router';
import { TrainerFormComponent } from './components/trainer-form/trainer-form';
import { RegistrationDashboardComponent } from './components/registration-dashboard/registration-dashboard';

export const routes: Routes = [
  { path: 'cadastros', component: TrainerFormComponent },
  { path: 'matriculas', component: RegistrationDashboardComponent },
  { path: '', redirectTo: '/cadastros', pathMatch:'full' }
];