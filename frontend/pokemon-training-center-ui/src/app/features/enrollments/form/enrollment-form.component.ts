import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { PokemonService } from '../../../core/services/pokemon.service';
import { PlanService } from '../../../core/services/plan.service';
import { EnrollmentService } from '../../../core/services/enrollment.service';
import { Pokemon } from '../../../shared/models/pokemon.model';
import { Plan } from '../../../shared/models/plan.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { RouterLink } from '@angular/router';

const ELITE_DOS_4_NIVEL_MINIMO = 50;

@Component({
  selector: 'app-enrollment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './enrollment-form.component.html',
  styleUrl: './enrollment-form.component.scss'
})
export class EnrollmentFormComponent implements OnInit {
  pokemons = signal<Pokemon[]>([]);
  plans = signal<Plan[]>([]);
  submitting = signal(false);

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private pokemonService: PokemonService,
    private planService: PlanService,
    private enrollmentService: EnrollmentService,
    private router: Router,
    private toastService: ToastService
  ) {}

  ngAfterContentInit(): void {
    this.form = this.fb.group({
      pokemonId: [null as number | null, Validators.required],
      planoId: [null as number | null, Validators.required],
      dataInicio: [new Date().toISOString().slice(0, 10), Validators.required]
    });
  }

  ngOnInit(): void {
    this.pokemonService.getAll().subscribe(data => this.pokemons.set(data));
    this.planService.getAll().subscribe(data => this.plans.set(data));
  }

  get pokemonSelecionado(): Pokemon | undefined {
    const id = this.form?.value.pokemonId;
    return this.pokemons().find(p => p.id === id);
  }

  get planoSelecionado(): Plan | undefined {
    const id = this.form?.value.planoId;
    return this.plans().find(p => p.id === id);
  }

  get nivelInsuficiente(): boolean {
    const pokemon = this.pokemonSelecionado;
    const plano = this.planoSelecionado;
    if (!pokemon || !plano) return false;
    return pokemon.nivel < plano.nivelMinimoRequerido;
  }

  submit(): void {
    if (this.form.invalid || this.nivelInsuficiente) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const { pokemonId, planoId, dataInicio } = this.form.value;

    this.enrollmentService
      .create({ pokemonId: pokemonId!, planoId: planoId!, dataInicio: dataInicio! })
      .subscribe({
        next: () => {
          this.toastService.show('Matrícula criada com sucesso!', 'success');
          this.router.navigate(['/matriculas']);
        },
        error: () => this.submitting.set(false)
      });
  }
}