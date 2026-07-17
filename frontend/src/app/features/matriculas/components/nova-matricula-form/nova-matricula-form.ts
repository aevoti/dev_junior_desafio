import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { Matricula, PlanoTreinamento, Pokemon } from '../../../../core/models/api.models';
import { obterMensagemErro } from '../../../../core/services/api-error';
import { MatriculaService } from '../../../../core/services/matricula.service';
import { PokemonAvatar } from '../../../../shared/components/pokemon-avatar/pokemon-avatar';

@Component({
  selector: 'app-nova-matricula-form',
  imports: [ReactiveFormsModule, CurrencyPipe, DatePipe, PokemonAvatar],
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
  private readonly cdr = inject(ChangeDetectorRef);
  enviando = false;
  readonly form = this.fb.nonNullable.group({
    pokemonId: [0, [Validators.required, Validators.min(1)]],
    planoTreinamentoId: [0, [Validators.required, Validators.min(1)]],
    dataInicio: [this.dataLocalHoje(), Validators.required]
  });

  get pokemonsOrdenados(): Pokemon[] {
    return [...this.pokemons].sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR'));
  }

  get pokemonSelecionado(): Pokemon | null {
    return this.pokemons.find(x => x.id === Number(this.form.controls.pokemonId.value)) ?? null;
  }

  get planoSelecionado(): PlanoTreinamento | null {
    return this.planos.find(x => x.id === Number(this.form.controls.planoTreinamentoId.value)) ?? null;
  }

  get dataInicioSelecionada(): Date | null {
    const value = this.form.controls.dataInicio.value;
    return value ? new Date(`${value}T12:00:00`) : null;
  }

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
    }).pipe(finalize(() => { this.enviando = false; this.cdr.markForCheck(); })).subscribe({
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
