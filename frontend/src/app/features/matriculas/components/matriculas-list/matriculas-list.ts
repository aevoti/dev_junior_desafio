import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Matricula } from '../../../../core/models/api.models';

@Component({
  selector: 'app-matriculas-list',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './matriculas-list.html',
  styleUrl: './matriculas-list.scss'
})
export class MatriculasList {
  @Input({ required: true }) matriculas: Matricula[] = [];
  @Output() upgrade = new EventEmitter<Matricula>();

  rotuloStatus(status: Matricula['status']): string {
    return status === 'Concluida' ? 'Concluída' : status;
  }
}
