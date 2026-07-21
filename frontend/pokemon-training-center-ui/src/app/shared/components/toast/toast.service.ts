import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
  id: number;
  text: string;
  type: 'error' | 'success';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  messages = signal<ToastMessage[]>([]);
  private nextId = 0;

  show(text: string, type: 'error' | 'success' = 'error'): void {
    const id = this.nextId++;
    this.messages.update(list => [...list, { id, text, type }]);
    setTimeout(() => this.dismiss(id), 5000);
  }

  dismiss(id: number): void {
    this.messages.update(list => list.filter(m => m.id !== id));
  }
}