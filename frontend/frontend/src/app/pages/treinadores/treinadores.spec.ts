import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Treinadores } from './treinadores';

describe('Treinadores', () => {
  let component: Treinadores;
  let fixture: ComponentFixture<Treinadores>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Treinadores],
    }).compileComponents();

    fixture = TestBed.createComponent(Treinadores);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
