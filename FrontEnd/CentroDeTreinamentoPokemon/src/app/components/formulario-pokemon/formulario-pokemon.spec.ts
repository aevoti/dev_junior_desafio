import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FormularioPokemon } from './formulario-pokemon';

describe('FormularioPokemon', () => {
  let component: FormularioPokemon;
  let fixture: ComponentFixture<FormularioPokemon>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormularioPokemon],
    }).compileComponents();

    fixture = TestBed.createComponent(FormularioPokemon);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
