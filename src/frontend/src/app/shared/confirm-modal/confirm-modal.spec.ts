import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { ConfirmModal } from './confirm-modal';

describe('ConfirmModal', () => {
  function createFixture() {
    const fixture = TestBed.createComponent(ConfirmModal);
    fixture.componentRef.setInput('title', 'Genre löschen');
    fixture.componentRef.setInput('message', 'Soll „Rock" wirklich gelöscht werden?');
    fixture.detectChanges();
    return fixture;
  }

  it('zeigt Titel und Nachricht an', () => {
    // arrange
    const fixture = createFixture();

    // act
    const compiled = fixture.nativeElement as HTMLElement;

    // assert
    expect(compiled.textContent).toContain('Genre löschen');
    expect(compiled.textContent).toContain('Soll „Rock" wirklich gelöscht werden?');
  });

  it('emittiert confirmed beim Klick auf Löschen', () => {
    // arrange
    const fixture = createFixture();
    const confirmedHandler = vi.fn();
    fixture.componentInstance.confirmed.subscribe(confirmedHandler);
    const button = (fixture.nativeElement as HTMLElement).querySelector(
      '.btn-danger',
    ) as HTMLButtonElement;

    // act
    button.click();

    // assert
    expect(confirmedHandler).toHaveBeenCalledTimes(1);
  });

  it('emittiert cancelled beim Klick auf Abbrechen', () => {
    // arrange
    const fixture = createFixture();
    const cancelledHandler = vi.fn();
    fixture.componentInstance.cancelled.subscribe(cancelledHandler);
    const button = (fixture.nativeElement as HTMLElement).querySelector(
      '.btn-secondary',
    ) as HTMLButtonElement;

    // act
    button.click();

    // assert
    expect(cancelledHandler).toHaveBeenCalledTimes(1);
  });

  it('zeigt ein alternatives Bestätigen-Label und eine alternative Variante an', () => {
    // arrange
    const fixture = TestBed.createComponent(ConfirmModal);
    fixture.componentRef.setInput('title', 'Künstler anlegen');
    fixture.componentRef.setInput('message', 'Soll der Künstler „Neu" neu angelegt werden?');
    fixture.componentRef.setInput('confirmLabel', 'Anlegen');
    fixture.componentRef.setInput('confirmVariant', 'primary');
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    // act
    const button = Array.from(compiled.querySelectorAll('button')).find(
      (b) => b.textContent?.trim() === 'Anlegen',
    );

    // assert
    expect(button).toBeDefined();
    expect(button?.classList.contains('btn-primary')).toBe(true);
    expect(button?.classList.contains('btn-danger')).toBe(false);
  });

  it('emittiert cancelled, wenn das Modal geschlossen wird (Escape/Scrim)', () => {
    // arrange
    const fixture = createFixture();
    const cancelledHandler = vi.fn();
    fixture.componentInstance.cancelled.subscribe(cancelledHandler);

    // act
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));

    // assert
    // ConfirmModal hat keinen eigenen Abbruch-Zustand — ein Schließen des Wrapper-Modals
    // (Escape/Scrim) ist fachlich identisch zum expliziten Abbrechen.
    expect(cancelledHandler).toHaveBeenCalledTimes(1);
  });
});
