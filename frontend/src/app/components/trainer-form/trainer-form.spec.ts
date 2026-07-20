import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrainerFormComponent } from './trainer-form';

describe('TrainerFormComponent', () => {
  let component: TrainerFormComponent;
  let fixture: ComponentFixture<TrainerFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TrainerFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TrainerFormComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
