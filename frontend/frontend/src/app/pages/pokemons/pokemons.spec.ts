import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Pokemons } from './pokemons';

describe('Pokemons', () => {
  let component: Pokemons;
  let fixture: ComponentFixture<Pokemons>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Pokemons],
    }).compileComponents();

    fixture = TestBed.createComponent(Pokemons);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
