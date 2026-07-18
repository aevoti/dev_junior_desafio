import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../services/api.service';
import { Matricula, STATUS_LIST, PLANOS, UpgradePreview } from '../../../models/matricula';

@Component({
  selector: 'app-lista-matriculas',
  templateUrl: './lista-matriculas.component.html',
  styleUrls: ['./lista-matriculas.component.css']
})
export class ListaMatriculasComponent implements OnInit {
  matriculas: Matricula[] = [];
  busca = '';
  statusFiltro = '';
  statusList = ['', ...STATUS_LIST];
  planos = PLANOS;
  carregando = false;
  erro = '';

  // Estado do modal de upgrade
  matriculaSelecionada: Matricula | null = null;
  novoPlanoUpgrade = '';
  upgradePreview: UpgradePreview | null = null;
  upgradeErro = '';
  upgradeCarregando = false;
  upgradeConfirmado = false;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.carregando = true;
    this.erro = '';
    this.api.getMatriculas(this.busca || undefined, this.statusFiltro || undefined).subscribe({
      next: (data) => { this.matriculas = data; this.carregando = false; },
      error: () => { this.erro = 'Erro ao carregar matrículas. Verifique se a API está rodando.'; this.carregando = false; }
    });
  }

  cancelar(id: number): void {
    if (!confirm('Deseja cancelar esta matrícula?')) return;
    this.api.cancelarMatricula(id).subscribe({
      next: () => this.carregar(),
      error: (e) => { this.erro = e.error?.erro || 'Erro ao cancelar matrícula.'; }
    });
  }

  abrirUpgrade(matricula: Matricula): void {
    this.matriculaSelecionada = matricula;
    this.novoPlanoUpgrade = '';
    this.upgradePreview = null;
    this.upgradeErro = '';
    this.upgradeConfirmado = false;
  }

  fecharUpgrade(): void {
    this.matriculaSelecionada = null;
    this.upgradePreview = null;
    this.upgradeErro = '';
  }

  calcularPreview(): void {
    if (!this.matriculaSelecionada || !this.novoPlanoUpgrade) return;
    this.upgradeCarregando = true;
    this.upgradeErro = '';
    this.upgradePreview = null;
    this.api.previewUpgrade(this.matriculaSelecionada.id, this.novoPlanoUpgrade).subscribe({
      next: (p) => { this.upgradePreview = p; this.upgradeCarregando = false; },
      error: (e) => { this.upgradeErro = e.error?.erro || 'Não foi possível calcular o upgrade.'; this.upgradeCarregando = false; }
    });
  }

  confirmarUpgrade(): void {
    if (!this.matriculaSelecionada || !this.novoPlanoUpgrade) return;
    this.upgradeCarregando = true;
    this.api.executarUpgrade(this.matriculaSelecionada.id, this.novoPlanoUpgrade).subscribe({
      next: () => {
        this.upgradeConfirmado = true;
        this.upgradeCarregando = false;
        setTimeout(() => { this.fecharUpgrade(); this.carregar(); }, 1500);
      },
      error: (e) => { this.upgradeErro = e.error?.erro || 'Erro ao realizar upgrade.'; this.upgradeCarregando = false; }
    });
  }

  planosSuperiores(planoAtual: string): typeof PLANOS {
    const atual = PLANOS.findIndex(p => p.valor === planoAtual);
    return PLANOS.filter((_, i) => i > atual);
  }

  planoLabel(valor: string): string {
    return PLANOS.find(p => p.valor === valor)?.label ?? valor;
  }

  statusClass(status: string): string {
    return { 'Ativa': 'badge-ativa', 'Cancelada': 'badge-cancelada', 'Concluida': 'badge-concluida' }[status] ?? '';
  }
}
