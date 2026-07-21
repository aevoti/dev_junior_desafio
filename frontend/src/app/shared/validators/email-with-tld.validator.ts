import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// FR-024: Validators.email do Angular aceita endereços sem TLD (ex.: "nome@dominio").
// Exige um domínio com TLD de pelo menos 2 caracteres (ex.: "nome@dominio.com").
const EMAIL_WITH_TLD_REGEX = /^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$/;

export const emailWithTldValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  if (!control.value) {
    return null;
  }

  return EMAIL_WITH_TLD_REGEX.test(control.value) ? null : { emailWithTld: true };
};
