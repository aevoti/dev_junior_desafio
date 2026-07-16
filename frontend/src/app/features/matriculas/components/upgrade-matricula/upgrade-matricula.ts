import { CurrencyPipe } from '@angular/common';
import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { Matricula, PlanoTreinamento, SimulacaoUpgrade } from '../../../../core/models/api.models';
import { obterMensagemErro } from '../../../../core/services/api-error';
import { MatriculaService } from '../../../../core/services/matricula.service';

@Component({
  selector: 'app-upgrade-matricula',
  imports: [ReactiveFormsModule, CurrencyPipe],
  templateUrl: './upgrade-matricula.html',
  styleUrl: './upgrade-matricula.scss'
})
export class UpgradeMatricula implements OnInit {
  @Input({ required: true }) matricula!: Matricula;
  @Input({ required: true }) planos: PlanoTreinamento[] = [];
  @Output() fechar = new EventEmitter<void>();
  @Output() concluido = new EventEmitter<void>();
  @Output() erro = new EventEmitter<string>();

  private readonly fb = inject(FormBuilder);
  private readonly service = inject(MatriculaService);
  planosSuperiores: PlanoTreinamento[] = [];
  simulacao: SimulacaoUpgrade | null = null;
  processando = false;
  readonly form = this.fb.nonNullable.group({ novoPlanoTreinamentoId: [0, [Validators.required, Validators.min(1)]] });

  ngOnInit(): void {
    const atual = this.planos.find(x => x.id === this.matricula.planoTreinamentoId);
    this.planosSuperiores = atual ? this.planos.filter(x => x.ordem > atual.ordem) : [];
    this.form.controls.novoPlanoTreinamentoId.valueChanges.subscribe(() => this.simulacao = null);
  }

  simular(): void {
    if (this.form.invalid || this.processando) return;
    this.processando = true;
    this.service.simularUpgrade(this.matricula.id, {
      novoPlanoTreinamentoId: Number(this.form.controls.novoPlanoTreinamentoId.value)
    }).pipe(finalize(() => this.processando = false)).subscribe({
      next: simulacao => this.simulacao = simulacao,
      error: error => this.erro.emit(obterMensagemErro(error))
    });
  }

  confirmar(): void {
    if (!this.simulacao || this.processando) return;
    this.processando = true;
    this.service.realizarUpgrade(this.matricula.id, {
      novoPlanoTreinamentoId: this.simulacao.novoPlanoId
    }).pipe(finalize(() => this.processando = false)).subscribe({
      next: () => this.concluido.emit(),
      error: error => this.erro.emit(obterMensagemErro(error))
    });
  }
}
