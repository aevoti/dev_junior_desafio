import { ChangeDetectorRef, Component, EventEmitter, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-registration-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './registration-form.html',
  styleUrl: './registration-form.css'
})
export class RegistrationFormComponent implements OnInit {
  @Output() registered = new EventEmitter<void>();

  registration: {pokemonId: number | null, trainingPlan: string, initDate: string} = {
    pokemonId: null,
    trainingPlan: '',
    initDate: ''
  };

  pokemonsList: any[] = [];

  message: string = '';
  isError: boolean = false;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadPokemons();
  }

  loadPokemons() {
    this.http.get<any[]>('http://localhost:5141/api/pokemon').subscribe({
      next: (data) => {
        this.pokemonsList = data;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erro ao carregar pokemons:', err);
      }
    })
  }

  onSubmit() {
    if (!this.registration.initDate || 
        (typeof this.registration.initDate === 'string' && !this.registration.initDate.trim())) 
    {
      this.isError = true;
      this.message = 'Preencha todos os campos';
      this.cdr.detectChanges();
      return;
    }

    const apiUrl = 'http://localhost:5141/api/registration';

    this.http.post(apiUrl, this.registration).subscribe({
      next: (response: any) => {
        this.message = response.message;
        this.isError = false;
        this.registration = { pokemonId: null, trainingPlan: '', initDate: ''};
        this.registered.emit();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isError = true;
        this.message = err.error?.message || 'Erro ao conectar com o servidor.';
        this.cdr.detectChanges();
      }
    });
  }
}