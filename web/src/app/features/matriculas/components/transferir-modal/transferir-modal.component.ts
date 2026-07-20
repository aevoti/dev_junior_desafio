import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PokemonService } from '../../../../core/services/pokemon.service';
import { TreinadorService } from '../../../../core/services/treinador.service';
import { Treinador } from '../../../../core/models/treinador.model';

/** Modal da transferência de Pokémon entre Treinadores (R5). */
@Component({
  selector: 'app-transferir-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transferir-modal.component.html',
  styleUrl: './transferir-modal.component.scss',
})
export class TransferirModalComponent implements OnInit {
  private readonly pokemonService = inject(PokemonService);
  private readonly treinadorService = inject(TreinadorService);

  @Input({ required: true }) pokemonId!: number;
  @Input({ required: true }) pokemonNome!: string;
  @Input({ required: true }) treinadorAtualId!: number;
  @Output() confirmado = new EventEmitter<void>();
  @Output() fechado = new EventEmitter<void>();

  treinadores: Treinador[] = [];
  novoTreinadorId: number | null = null;
  erro: string | null = null;
  confirmando = false;

  ngOnInit(): void {
    this.treinadorService.listar().subscribe((treinadores) => {
      this.treinadores = treinadores.filter((t) => t.id !== this.treinadorAtualId);
    });
  }

  confirmar(): void {
    if (!this.novoTreinadorId) return;

    this.erro = null;
    this.confirmando = true;
    this.pokemonService.transferir(this.pokemonId, { novoTreinadorId: this.novoTreinadorId }).subscribe({
      next: () => {
        this.confirmando = false;
        this.confirmado.emit();
      },
      error: (err: Error) => {
        this.erro = err.message;
        this.confirmando = false;
      },
    });
  }
}
