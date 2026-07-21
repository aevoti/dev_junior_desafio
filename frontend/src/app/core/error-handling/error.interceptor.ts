import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

// constante com a mensagem padrão exibida quando a API não retorna um erro no formato esperado
// (ou quando o erro não tem message, como em falhas de rede)
const GENERIC_ERROR_MESSAGE = 'Não foi possível completar a operação. Tente novamente em instantes.';

// interceptor registrado em app.config.ts
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  // next(req) executa a requisição, depois .pipe(catchError()) intercepta qualquer erro
  // emitido no Observable da response antes que ele chegue ao service que fez a chamada
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // extrai a message do corpo da resposta (API já retorna em português)
      // ou a mensagem genérica como fallback
      const message = error.error?.message ?? GENERIC_ERROR_MESSAGE;
      // relança o error como um Error comum do JS contendo a mensagem tratada
      // service nunca precisa lidar com HttpErrorResponse
      return throwError(() => new Error(message));
    })
  );
};
