import { CurrencyPipe } from '@angular/common';
import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { Matricula, PlanoTreinamento, Pokemon } from '../../../../core/models/api.models';
import { obterMensagemErro } from '../../../../core/services/api-error';
import { MatriculaService } from '../../../../core/services/matricula.service';

@Component({
  selector: 'app-nova-matricula-form',
  imports: [ReactiveFormsModule, CurrencyPipe],
  templateUrl: './nova-matricula-form.html',
  styleUrl: './nova-matricula-form.scss'
})
export class NovaMatriculaForm {
  @Input({ required: true }) pokemons: Pokemon[] = [];
  @Input({ required: true }) planos: PlanoTreinamento[] = [];
  @Output() fechar = new EventEmitter<void>();
  @Output() criada = new EventEmitter<Matricula>();
  @Output() erro = new EventEmitter<string>();

  private readonly fb = inject(FormBuilder);
  private readonly service = inject(MatriculaService);
  enviando = false;
  readonly form = this.fb.nonNullable.group({
    pokemonId: [0, [Validators.required, Validators.min(1)]],
    planoTreinamentoId: [0, [Validators.required, Validators.min(1)]],
    dataInicio: [this.dataLocalHoje(), Validators.required]
  });

  get nivelInsuficiente(): boolean {
    const pokemon = this.pokemons.find(x => x.id === Number(this.form.controls.pokemonId.value));
    const plano = this.planos.find(x => x.id === Number(this.form.controls.planoTreinamentoId.value));
    return !!pokemon && !!plano && pokemon.nivel < plano.nivelMinimo;
  }

  enviar(): void {
    if (this.form.invalid || this.nivelInsuficiente || this.enviando) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.enviando = true;
    this.service.criar({
      pokemonId: Number(value.pokemonId),
      planoTreinamentoId: Number(value.planoTreinamentoId),
      dataInicio: new Date(`${value.dataInicio}T12:00:00`).toISOString()
    }).pipe(finalize(() => this.enviando = false)).subscribe({
      next: matricula => this.criada.emit(matricula),
      error: error => this.erro.emit(obterMensagemErro(error))
    });
  }

  private dataLocalHoje(): string {
    const hoje = new Date();
    const offset = hoje.getTimezoneOffset() * 60000;
    return new Date(hoje.getTime() - offset).toISOString().slice(0, 10);
  }
}
