import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FormularioMatricula } from './formulario-matricula';

describe('FormularioMatricula', () => {
  let component: FormularioMatricula;
  let fixture: ComponentFixture<FormularioMatricula>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormularioMatricula],
    }).compileComponents();

    fixture = TestBed.createComponent(FormularioMatricula);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
