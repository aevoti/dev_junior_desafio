import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { ListaDeMatriculas } from './lista-de-matriculas';

describe('ListaDeMatriculas', () => {
  let component: ListaDeMatriculas;
  let fixture: ComponentFixture<ListaDeMatriculas>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListaDeMatriculas],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(ListaDeMatriculas);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
