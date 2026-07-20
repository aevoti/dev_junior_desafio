import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RegistrationFormComponent } from '../registration-form/registration-form';
import { HttpClient } from '@angular/common/http';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReplaceTrainerFormComponent } from '../replace-trainer-form/replace-trainer-form';

@Component({
  selector: 'app-registration-dashboard',
  standalone: true,
  imports: [RegistrationFormComponent, CurrencyPipe, FormsModule, ReplaceTrainerFormComponent],
  templateUrl: './registration-dashboard.html',
  styleUrl: './registration-dashboard.css',
})
export class RegistrationDashboardComponent implements OnInit {
  registrationList: any[] = [];
  filterText: string = '';
  filterStatus: string = '';

  message: string = '';
  isError: boolean = false;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadRegistrations();
  }

  loadRegistrations() {
    const url = `http://localhost:5141/api/registration?search=${this.filterText}&status=${this.filterStatus}`;

    this.http.get<any[]>(url).subscribe({
      next: (data) => {
        this.registrationList = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erro ao buscar matrículas:', err);
      }
    });
  }

  cancelRegistration(id: number) {
    const url = `http://localhost:5141/api/registration/${id}/cancel`;

    this.http.put(url, {}).subscribe({
      next: (response: any) => {
        alert(response.message);
        this.loadRegistrations();
      },
      error: (err) => {
        alert(err.error?.message || 'Erro ao cancelar matrícula.');
      }
    });
  }

  upgradeRegistration(id: number, planoAtual: string) {
    let novoPlano = '';

    if (planoAtual === 'Ginasio Local') {
      novoPlano = 'Liga Regional';
    } else if (planoAtual === 'Liga Regional') {
      novoPlano = 'Elite dos 4';
    } else {
      alert('Este Pokémon já está no plano máximo (Elite dos 4)!');
      return;
    }

    const urlSimulacao = `http://localhost:5141/api/registration/${id}/upgrade?commit=false`;
    const urlConfirmacao = `http://localhost:5141/api/registration/${id}/upgrade?commit=true`;
    
    const headers = { 'Content-Type': 'application/json' };
    const body = JSON.stringify(novoPlano);

    // call api only to see the value
    this.http.post(urlSimulacao, body, { headers }).subscribe({
      next: (simulacao: any) => {
        
        // show value before confirm
        const confirmou = confirm(
          `Extrato de Pro-rata do Upgrade:\n\n` +
          `Mudança: ${planoAtual} ➔ ${novoPlano}\n` +
          `Valor da primeira cobrança proporcional: R$ ${simulacao.valorCobrado.toFixed(2)}\n\n` +
          `Deseja confirmar e aplicar este upgrade agora?`
        );

        // if it is cancelled, the db is not modified
        if (!confirmou) return;

        // if confirm, updates db
        this.http.post(urlConfirmacao, body, { headers }).subscribe({
          next: (resultado: any) => {
            alert(resultado.message);
            this.loadRegistrations();
          },
          error: (err) => alert(err.error?.message || 'Erro ao efetivar upgrade.')
        });

      },
      error: (err) => {
        alert(err.error?.message || 'Erro ao simular upgrade.');
      }
    });
  }
}
