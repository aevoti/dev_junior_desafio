import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { TreinadorService } from '../../services/treinador.service';

@Component({
  selector: 'app-formulario-treinador',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './formulario-treinador.html',
  styleUrl: './formulario-treinador.css',
})
export class FormularioTreinador {
  private readonly treinadorService = inject(TreinadorService);

  form = new FormGroup({
    nome: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(150)],
    }),
    cidadeDeOrigem: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
  });

  carregando = signal(false);
  erro = signal<string | null>(null);
  sucesso = signal(false);

  enviar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.carregando.set(true);
    this.erro.set(null);
    this.sucesso.set(false);

    this.treinadorService.criar(this.form.getRawValue()).subscribe({
      next: () => {
        this.carregando.set(false);
        this.sucesso.set(true);
        this.form.reset();
      },
      error: (erro: HttpErrorResponse) => {
        this.carregando.set(false);
        this.erro.set(erro.error?.mensagem ?? 'Ocorreu um erro inesperado. Tente novamente.');
      },
    });
  }
}
