import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';

import { PlanoTreinamentoService } from '../../core/services/plano-treinamento.service';
import { PlanoTreinamentoResponse } from '../../models/plano-treinamento/plano-treinamento-response';

@Component({
  selector: 'app-planos',
  imports: [
    CommonModule
  ],
  templateUrl: './planos.html',
  styleUrl: './planos.scss'
})
export class Planos implements OnInit {
  planos: PlanoTreinamentoResponse[] = [];

  carregando: boolean = false;
  mensagemErro: string = '';

  constructor(
    private readonly planoTreinamentoService: PlanoTreinamentoService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.carregarPlanos();
  }

  carregarPlanos(): void {
    this.carregando = true;
    this.mensagemErro = '';

    this.planoTreinamentoService
      .listar()
      .subscribe({
        next: (planos: PlanoTreinamentoResponse[]) => {
          this.planos = planos.sort(
  (
            primeiroPlano: PlanoTreinamentoResponse,
            segundoPlano: PlanoTreinamentoResponse
          ) => primeiroPlano.nivelPlano - segundoPlano.nivelPlano
        );

          this.carregando = false;

          this.changeDetectorRef.detectChanges();
        },
        error: (erro: HttpErrorResponse) => {
          console.error('Erro ao carregar planos:', erro);

          this.mensagemErro =
            'Não foi possível carregar os planos de treinamento.';

          this.carregando = false;

          this.changeDetectorRef.detectChanges();
        }
      });
  }
}