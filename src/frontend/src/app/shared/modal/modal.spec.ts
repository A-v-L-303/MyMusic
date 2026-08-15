import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';

import { Modal } from './modal';

describe('Modal', () => {
  it('setzt standardmäßig keine breite Modal-Klasse', () => {
    // arrange
    const fixture = TestBed.createComponent(Modal);

    // act
    fixture.detectChanges();

    // assert
    const panel = (fixture.nativeElement as HTMLElement).querySelector('.modal') as HTMLElement;
    expect(panel.classList.contains('modal-wide')).toBe(false);
  });

  it('setzt die breite Modal-Klasse, wenn wide auf true steht', () => {
    // arrange
    const fixture = TestBed.createComponent(Modal);
    fixture.componentRef.setInput('wide', true);

    // act
    fixture.detectChanges();

    // assert
    const panel = (fixture.nativeElement as HTMLElement).querySelector('.modal') as HTMLElement;
    expect(panel.classList.contains('modal-wide')).toBe(true);
  });

  it('emittiert closed bei Klick auf den Scrim', () => {
    // arrange
    const fixture = TestBed.createComponent(Modal);
    fixture.detectChanges();
    const closedHandler = vi.fn();
    fixture.componentInstance.closed.subscribe(closedHandler);
    const scrim = (fixture.nativeElement as HTMLElement).querySelector('.scrim') as HTMLElement;

    // act
    scrim.dispatchEvent(new MouseEvent('click', { bubbles: true }));

    // assert
    expect(closedHandler).toHaveBeenCalledTimes(1);
  });

  it('emittiert closed nicht bei Klick innerhalb des Panels', () => {
    // arrange
    const fixture = TestBed.createComponent(Modal);
    fixture.detectChanges();
    const closedHandler = vi.fn();
    fixture.componentInstance.closed.subscribe(closedHandler);
    const panel = (fixture.nativeElement as HTMLElement).querySelector('.modal') as HTMLElement;

    // act
    panel.dispatchEvent(new MouseEvent('click', { bubbles: true }));

    // assert
    // Klick-Ziel ist das Panel selbst, nicht der Scrim — onScrimClick() prüft target === currentTarget.
    expect(closedHandler).not.toHaveBeenCalled();
  });

  it('emittiert closed bei Escape-Taste', () => {
    // arrange
    const fixture = TestBed.createComponent(Modal);
    fixture.detectChanges();
    const closedHandler = vi.fn();
    fixture.componentInstance.closed.subscribe(closedHandler);

    // act
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));

    // assert
    expect(closedHandler).toHaveBeenCalledTimes(1);
  });

  it('schließt bei zwei gleichzeitig offenen Modals per Escape nur das zuletzt geöffnete', () => {
    // arrange
    const outerFixture = TestBed.createComponent(Modal);
    outerFixture.detectChanges();
    const outerClosedHandler = vi.fn();
    outerFixture.componentInstance.closed.subscribe(outerClosedHandler);

    const innerFixture = TestBed.createComponent(Modal);
    innerFixture.detectChanges();
    const innerClosedHandler = vi.fn();
    innerFixture.componentInstance.closed.subscribe(innerClosedHandler);

    // act: erstes Escape trifft nur das obere (zuletzt erzeugte) Modal
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));

    // assert
    expect(innerClosedHandler).toHaveBeenCalledTimes(1);
    expect(outerClosedHandler).not.toHaveBeenCalled();

    // act: nach dem Entfernen des oberen Modals trifft Escape jetzt das untere
    innerFixture.destroy();
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));

    // assert
    expect(outerClosedHandler).toHaveBeenCalledTimes(1);
  });
});
