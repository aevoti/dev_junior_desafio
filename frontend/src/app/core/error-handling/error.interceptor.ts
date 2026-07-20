import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

const GENERIC_ERROR_MESSAGE = 'Não foi possível completar a operação. Tente novamente em instantes.';

/**
 * Traduz respostas de erro da API (`{ "message": "..." }`, já em português —
 * ver contracts/api.md) em um `Error` com mensagem pronta para exibição
 * amigável nos componentes (R1, R2, R3).
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const message = error.error?.message ?? GENERIC_ERROR_MESSAGE;
      return throwError(() => new Error(message));
    })
  );
};
