import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReplaceTrainerForm } from './replace-trainer-form';

describe('ReplaceTrainerForm', () => {
  let component: ReplaceTrainerForm;
  let fixture: ComponentFixture<ReplaceTrainerForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReplaceTrainerForm],
    }).compileComponents();

    fixture = TestBed.createComponent(ReplaceTrainerForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
