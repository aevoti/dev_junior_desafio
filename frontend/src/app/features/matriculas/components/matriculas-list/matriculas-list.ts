import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Matricula, Pokemon } from '../../../../core/models/api.models';
import { PokemonAvatar } from '../../../../shared/components/pokemon-avatar/pokemon-avatar';
import { splitPokemonName } from '../../../../shared/utils/pokemon-name.util';

@Component({
  selector: 'app-matriculas-list',
  imports: [CurrencyPipe, DatePipe, PokemonAvatar],
  templateUrl: './matriculas-list.html',
  styleUrl: './matriculas-list.scss'
})
export class MatriculasList {
  @Input({ required: true }) matriculas: Matricula[] = [];
  @Input({ required: true }) pokemons: Pokemon[] = [];
  @Output() upgrade = new EventEmitter<Matricula>();

  rotuloStatus(status: Matricula['status']): string {
    return status === 'Concluida' ? 'Concluída' : status;
  }

  nivelPokemon(id: number): number | null {
    return this.pokemons.find(pokemon => pokemon.id === id)?.nivel ?? null;
  }

  nomeBase(nome: string): string {
    return splitPokemonName(nome).base;
  }

  complementoNome(nome: string): string {
    return splitPokemonName(nome).complement;
  }

  classeNivel(nivel: number): string {
    if (nivel >= 80) return 'level-gold';
    if (nivel >= 50) return 'level-purple';
    if (nivel >= 20) return 'level-blue';
    return 'level-neutral';
  }
}
