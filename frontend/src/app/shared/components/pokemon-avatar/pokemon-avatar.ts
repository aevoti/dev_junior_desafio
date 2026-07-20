import { ChangeDetectorRef, Component, inject, Input, OnChanges, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { PokemonVisual } from '../../../core/models/poke-api.models';
import { PokeApiService } from '../../../core/services/poke-api.service';

@Component({
  selector: 'app-pokemon-avatar',
  templateUrl: './pokemon-avatar.html',
  styleUrl: './pokemon-avatar.scss'
})
export class PokemonAvatar implements OnChanges, OnDestroy {
  @Input({ required: true }) nome = '';
  @Input() tamanho = 64;
  visual: PokemonVisual | null = null;
  carregando = true;
  private readonly service = inject(PokeApiService);
  private readonly cdr = inject(ChangeDetectorRef);
  private subscription?: Subscription;

  ngOnChanges(): void {
    this.subscription?.unsubscribe();
    this.carregando = true;
    this.visual = null;
    this.subscription = this.service.buscar(this.nome).subscribe(visual => {
      this.visual = visual;
      this.carregando = false;
      this.cdr.markForCheck();
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  traduzirTipo(tipo: string): string {
    const traducoes: Record<string, string> = {
      electric: 'Elétrico', fire: 'Fogo', water: 'Água', grass: 'Planta', normal: 'Normal',
      psychic: 'Psíquico', fighting: 'Lutador', ground: 'Terrestre', rock: 'Pedra', ghost: 'Fantasma',
      ice: 'Gelo', dragon: 'Dragão', dark: 'Sombrio', fairy: 'Fada', steel: 'Aço', bug: 'Inseto',
      poison: 'Venenoso', flying: 'Voador'
    };
    return traducoes[tipo] ?? tipo;
  }

  classeTipo(tipo: string): string {
    return `type type-${tipo.replace(/[^a-z-]/g, '')}`;
  }
}
