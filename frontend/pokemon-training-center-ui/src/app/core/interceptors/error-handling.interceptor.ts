import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../../shared/components/toast/toast.service';

export const errorHandlingInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const mensagem = error.error?.mensagem
        ?? 'Ocorreu um erro inesperado. Tente novamente.';

      toastService.show(mensagem, 'error');
      return throwError(() => error);
    })
  );
};