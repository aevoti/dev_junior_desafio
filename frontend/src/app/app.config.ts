import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { errorInterceptor } from './core/error-handling/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // Zoneless: sem a biblioteca zone.js, que vigia tudo que pode mudar o estado
    // (cliques, respostas de requisições HTTP, etc.) e notifica o Angular, que
    // varre os componentes para atualizar o que for preciso. Componentes só atualizam quando
    // um signal muda ou quando ocorre um evento de template já rastreado pelo Angular
    // (ex: (click)).
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([errorInterceptor]))
  ]
};
