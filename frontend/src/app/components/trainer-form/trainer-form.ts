import { ChangeDetectorRef, Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { PokemonFormComponent } from '../pokemon-form/pokemon-form';

@Component({
  selector: 'app-trainer-form',
  standalone: true,
  imports: [CommonModule, FormsModule, PokemonFormComponent],
  templateUrl: './trainer-form.html',
  styleUrl: './trainer-form.css'
})
export class TrainerFormComponent {
  trainer = {
    Name: '',
    Email: '',
    OriginCity: ''
  };

  message: string = '';
  isError: boolean = false;

  @Output() trainerCreated = new EventEmitter<void>();

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  onSubmit() {

    const apiUrl = 'http://localhost:5141/api/trainer';

    this.http.post(apiUrl, this.trainer).subscribe({
      next: (response: any) => {
        this.message = response.message;
        this.isError = false;
        this.trainer = { Name: '', Email: '', OriginCity: '' };

        this.trainerCreated.emit();
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