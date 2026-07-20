import { ChangeDetectorRef, Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { Pokemon, Treinador } from '../../../../core/models/api.models';
import { obterMensagemErro } from '../../../../core/services/api-error';
import { PokemonService } from '../../../../core/services/pokemon.service';
import { PokemonAvatar } from '../../../../shared/components/pokemon-avatar/pokemon-avatar';

@Component({
  selector: 'app-transferir-pokemon',
  imports: [ReactiveFormsModule, PokemonAvatar],
  templateUrl: './transferir-pokemon.html',
  styleUrl: './transferir-pokemon.scss'
})
export class TransferirPokemon {
  @Input({ required: true }) pokemon!: Pokemon;
  @Input({ required: true }) treinadores: Treinador[] = [];
  @Output() fechar = new EventEmitter<void>();
  @Output() transferido = new EventEmitter<Pokemon>();
  @Output() erro = new EventEmitter<string>();

  private readonly fb = inject(FormBuilder);
  private readonly service = inject(PokemonService);
  private readonly cdr = inject(ChangeDetectorRef);
  processando = false;
  readonly form = this.fb.nonNullable.group({
    novoTreinadorId: [0, [Validators.required, Validators.min(1)]]
  });

  get treinadoresDisponiveis(): Treinador[] {
    return this.treinadores
      .filter(treinador => treinador.id !== this.pokemon.treinadorId)
      .sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR'));
  }

  confirmar(): void {
    if (this.form.invalid || this.processando) return;
    this.processando = true;
    this.service.transferir(this.pokemon.id, {
      novoTreinadorId: Number(this.form.controls.novoTreinadorId.value)
    }).pipe(finalize(() => { this.processando = false; this.cdr.markForCheck(); })).subscribe({
      next: pokemon => this.transferido.emit(pokemon),
      error: error => this.erro.emit(obterMensagemErro(error))
    });
  }
}
