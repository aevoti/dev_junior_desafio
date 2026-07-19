import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatriculaService } from '../../../../core/services/matricula.service';
import { Matricula, StatusMatricula } from '../../../../core/models/matricula.model';

/**
 * Tela 1 (listagem): busca por nome do Pokémon/Treinador + filtro por status.
 * TODO: fluxo de upgrade (abrir upgrade-modal) e ação de cancelar matrícula.
 */
@Component({
  selector: 'app-matriculas-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './matriculas-list.component.html',
})
export class MatriculasListComponent implements OnInit {
  private readonly matriculaService = inject(MatriculaService);

  matriculas: Matricula[] = [];
  busca = '';
  statusSelecionado: StatusMatricula | '' = '';
  carregando = false;
  erro: string | null = null;

  ngOnInit(): void {
    this.buscar();
  }

  buscar(): void {
    this.carregando = true;
    this.erro = null;
    this.matriculaService.listar(this.busca || undefined, this.statusSelecionado || undefined).subscribe({
      next: (matriculas) => {
        this.matriculas = matriculas;
        this.carregando = false;
      },
      error: (err: Error) => {
        this.erro = err.message;
        this.carregando = false;
      },
    });
  }
}
