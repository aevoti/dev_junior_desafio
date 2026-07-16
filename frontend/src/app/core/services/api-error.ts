import { HttpErrorResponse } from '@angular/common/http';

export function obterMensagemErro(error: unknown): string {
  if (error instanceof HttpErrorResponse && typeof error.error?.message === 'string') {
    return error.error.message;
  }
  return 'Não foi possível concluir a operação.';
}
