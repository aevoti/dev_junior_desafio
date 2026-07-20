import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

/**
 * Normaliza erros da API em { message } para exibição amigável nos componentes
 * (ex.: R1 - tentativa de matrícula duplicada). A UI de toast/snackbar em si
 * é responsabilidade de cada tela (ver TODO em matricula-form.component.ts).
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const mensagem = error.error?.message ?? 'Ocorreu um erro inesperado. Tente novamente.';
      return throwError(() => new Error(mensagem));
    }),
  );
