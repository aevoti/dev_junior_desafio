import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';

import { FormularioUpgradeMatricula } from './formulario-upgrade-matricula';

describe('FormularioUpgradeMatricula', () => {
  let component: FormularioUpgradeMatricula;
  let fixture: ComponentFixture<FormularioUpgradeMatricula>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormularioUpgradeMatricula],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: '1' }) } },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FormularioUpgradeMatricula);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
