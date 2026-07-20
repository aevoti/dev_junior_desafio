import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { PokemonPayload } from '../../models/pokemon.model';
import { Treinador } from '../../models/treinador.model';
import { PokemonService } from '../../services/pokemon.service';
import { TreinadorService } from '../../services/treinador.service';

@Component({
  selector: 'app-formulario-pokemon',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './formulario-pokemon.html',
  styleUrl: './formulario-pokemon.css',
})
export class FormularioPokemon {
  private readonly pokemonService = inject(PokemonService);
  private readonly treinadorService = inject(TreinadorService);

  form = new FormGroup({
    nome: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    tipo: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(50)],
    }),
    nivel: new FormControl(null, [Validators.required, Validators.min(1), Validators.max(100)]),
    treinadorId: new FormControl(null, Validators.required),
  });

  treinadores = signal<Treinador[]>([]);
  carregando = signal(false);
  erro = signal<string | null>(null);
  sucesso = signal(false);

  constructor() {
    this.treinadorService.listar().subscribe({
      next: (treinadores) => this.treinadores.set(treinadores),
    });
  }

  enviar(): void {
    if (this.form.invalid) {
      return;
    }

    this.carregando.set(true);
    this.erro.set(null);
    this.sucesso.set(false);

    const valores = this.form.getRawValue();
    const payload: PokemonPayload = {
      nome: valores.nome,
      tipo: valores.tipo,
      nivel: valores.nivel!,
      treinadorId: valores.treinadorId!,
    };

    this.pokemonService.criar(payload).subscribe({
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
