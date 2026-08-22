import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, isDevMode, signal } from '@angular/core';

import { ProblemDetails, ValidationProblemDetails } from '../http/problem-details';

export type ErrorModalKind =
  'not-found' | 'conflict' | 'server' | 'rate-limit' | 'network' | 'validation' | 'discogs';

export interface ErrorModalState {
  kind: ErrorModalKind;
  message: string;
  onRetry?: () => void;
}

@Injectable({ providedIn: 'root' })
export class ErrorModalService {
  private readonly state = signal<ErrorModalState | null>(null);

  readonly current = this.state.asReadonly();

  showFromHttpError(error: HttpErrorResponse, entityName: string, onRetry?: () => void): void {
    if (error.status === 401 || error.status === 403) {
      return;
    }

    if (isDevMode()) {
      console.error(error);
    }

    this.state.set(this.mapToState(error, entityName, onRetry));
  }

  showValidationMessage(message: string): void {
    this.state.set({ kind: 'validation', message });
  }

  dismiss(): void {
    this.state.set(null);
  }

  private mapToState(
    error: HttpErrorResponse,
    entityName: string,
    onRetry?: () => void,
  ): ErrorModalState {
    if (error.status === 0) {
      return {
        kind: 'network',
        message: 'Die Verbindung zum Server konnte nicht hergestellt werden.',
        onRetry,
      };
    }

    if (error.status === 404) {
      return { kind: 'not-found', message: `${entityName} wurde nicht gefunden.` };
    }

    if (error.status === 502) {
      return {
        kind: 'discogs',
        message: 'Discogs ist aktuell nicht erreichbar. Bitte die Daten manuell eingeben.',
      };
    }

    if (error.status === 400) {
      return { kind: 'validation', message: this.extractFirstValidationError(error) };
    }

    if (error.status === 409) {
      return {
        kind: 'conflict',
        message:
          this.extractDetail(error) ??
          `${entityName} steht im Konflikt mit einer bestehenden Ressource.`,
      };
    }

    if (error.status === 429) {
      return { kind: 'rate-limit', message: 'Zu viele Anfragen. Bitte versuche es später erneut.' };
    }

    return { kind: 'server', message: 'Es ist ein unerwarteter Serverfehler aufgetreten.' };
  }

  private extractDetail(error: HttpErrorResponse): string | undefined {
    return (error.error as ProblemDetails | undefined)?.detail;
  }

  private extractFirstValidationError(error: HttpErrorResponse): string {
    const errors = (error.error as ValidationProblemDetails | undefined)?.errors;
    const firstKey = errors ? Object.keys(errors)[0] : undefined;
    const firstMessage = firstKey ? errors?.[firstKey]?.[0] : undefined;

    return firstMessage ?? 'Die Eingabe ist ungültig.';
  }
}
