import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';
import { FormsModule } from '@angular/forms';

import { TreinadorService } from '../../core/services/treinador.service';
import { TreinadorRequest } from '../../models/treinador/treinador-request';
import { TreinadorResponse } from '../../models/treinador/treinador-response';

@Component({
  selector: 'app-treinadores',
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './treinadores.html',
  styleUrl: './treinadores.scss'
})
export class Treinadores implements OnInit {
  treinadores: TreinadorResponse[] = [];

  busca: string = '';

  nome: string = '';
  email: string = '';
  cidadeOrigem: string = '';

  carregando: boolean = false;
  salvando: boolean = false;
  mostrarFormulario: boolean = false;

  mensagemErro: string = '';
  mensagemSucesso: string = '';

  constructor(
    private readonly treinadorService: TreinadorService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.carregarTreinadores();
  }

  carregarTreinadores(): void {
    this.carregando = true;
    this.mensagemErro = '';

    this.treinadorService
      .listar(this.busca)
      .subscribe({
        next: (treinadores: TreinadorResponse[]) => {
          this.treinadores = treinadores;
          this.carregando = false;

          this.changeDetectorRef.detectChanges();
        },
        error: (erro: HttpErrorResponse) => {
          console.error('Erro ao carregar treinadores:', erro);

          this.mensagemErro =
            'Não foi possível carregar os treinadores.';

          this.carregando = false;

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  salvarTreinador(): void {
    this.mensagemErro = '';
    this.mensagemSucesso = '';

    if (
      !this.nome.trim() ||
      !this.email.trim() ||
      !this.cidadeOrigem.trim()
    ) {
      this.mensagemErro =
        'Preencha nome, e-mail e cidade de origem.';

      return;
    }

    const request: TreinadorRequest = {
      nome: this.nome.trim(),
      email: this.email.trim(),
      cidadeOrigem: this.cidadeOrigem.trim()
    };

    this.salvando = true;

    this.treinadorService
      .inserir(request)
      .subscribe({
        next: () => {
          this.mensagemSucesso =
            'Treinador cadastrado com sucesso.';

          this.salvando = false;
          this.mostrarFormulario = false;

          this.limparFormulario();
          this.carregarTreinadores();

          this.changeDetectorRef.detectChanges();
        },
        error: (erro: HttpErrorResponse) => {
          console.error('Erro ao cadastrar treinador:', erro);

          this.mensagemErro =
            erro.error?.mensagem ||
            erro.error?.message ||
            'Não foi possível cadastrar o treinador.';

          this.salvando = false;

          this.changeDetectorRef.detectChanges();
        }
      });
  }

  abrirFormulario(): void {
    this.mostrarFormulario = true;
    this.mensagemErro = '';
    this.mensagemSucesso = '';
  }

  fecharFormulario(): void {
    this.mostrarFormulario = false;
    this.limparFormulario();
  }

  limparFiltros(): void {
    this.busca = '';
    this.carregarTreinadores();
  }

  private limparFormulario(): void {
    this.nome = '';
    this.email = '';
    this.cidadeOrigem = '';
  }
}