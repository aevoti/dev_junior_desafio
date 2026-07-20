import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FormularioTreinador } from './formulario-treinador';

describe('FormularioTreinador', () => {
  let component: FormularioTreinador;
  let fixture: ComponentFixture<FormularioTreinador>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormularioTreinador],
    }).compileComponents();

    fixture = TestBed.createComponent(FormularioTreinador);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
