import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../../services/api.service';
import { Pokemon, PLANOS } from '../../../models/matricula';

@Component({
  selector: 'app-nova-matricula',
  templateUrl: './nova-matricula.component.html',
  styleUrls: ['./nova-matricula.component.css']
})
export class NovaMatriculaComponent implements OnInit {
  form: FormGroup;
  pokemons: Pokemon[] = [];
  planos = PLANOS;
  enviando = false;
  erro = '';
  sucesso = false;

  pokemonSelecionado: Pokemon | null = null;

  constructor(private fb: FormBuilder, private api: ApiService, private router: Router) {
    this.form = this.fb.group({
      pokemonId: [null, Validators.required],
      plano: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.api.getPokemons().subscribe({
      next: (data) => (this.pokemons = data),
      error: () => (this.erro = 'Erro ao carregar Pokémons.')
    });

    this.form.get('pokemonId')?.valueChanges.subscribe(id => {
      this.pokemonSelecionado = this.pokemons.find(p => p.id == id) ?? null;
      // Se Elite dos 4 selecionado mas pokémon não tem nível suficiente, limpa o plano
      if (this.form.value.plano === 'EliteDos4' && this.pokemonSelecionado && this.pokemonSelecionado.nivel < 50) {
        this.form.patchValue({ plano: '' });
      }
    });
  }

  planoDesabilitado(planoValor: string): boolean {
    if (planoValor === 'EliteDos4' && this.pokemonSelecionado && this.pokemonSelecionado.nivel < 50) {
      return true;
    }
    return false;
  }

  submeter(): void {
    if (this.form.invalid) return;

    // Validação Elite dos 4 no frontend (R3)
    if (this.form.value.plano === 'EliteDos4' && this.pokemonSelecionado && this.pokemonSelecionado.nivel < 50) {
      this.erro = 'Este Pokémon não atinge o nível mínimo (50) para o plano Elite dos 4.';
      return;
    }

    this.enviando = true;
    this.erro = '';

    const { pokemonId, plano } = this.form.value;
    this.api.criarMatricula(pokemonId, plano).subscribe({
      next: () => {
        this.sucesso = true;
        this.enviando = false;
        setTimeout(() => this.router.navigate(['/matriculas']), 1500);
      },
      error: (e) => {
        this.erro = e.error?.erro || 'Erro ao criar matrícula.';
        this.enviando = false;
      }
    });
  }
}
