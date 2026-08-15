import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { Country } from '../../../shared/country/country';
import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { Label } from '../label';
import { LabelService } from '../label.service';
import { LabelForm } from './label-form';

const countries: Country[] = [
  { id: 1, name: 'Deutschland', code: 'DE' },
  { id: 2, name: 'Vereinigtes Königreich', code: 'GB' },
];

describe('LabelForm', () => {
  let labelServiceMock: { create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let errorModalServiceMock: { showFromHttpError: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    labelServiceMock = { create: vi.fn(), update: vi.fn() };
    errorModalServiceMock = { showFromHttpError: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        { provide: LabelService, useValue: labelServiceMock },
        { provide: ErrorModalService, useValue: errorModalServiceMock },
      ],
    });
  });

  function createFixture(label: Label | null = null) {
    const fixture = TestBed.createComponent(LabelForm);
    fixture.componentRef.setInput('countries', countries);

    if (label) {
      fixture.componentRef.setInput('label', label);
    }

    fixture.detectChanges();
    return fixture;
  }

  function typeName(fixture: ReturnType<typeof createFixture>, value: string): void {
    const input = (fixture.nativeElement as HTMLElement).querySelector(
      '#label-name',
    ) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function selectCountry(fixture: ReturnType<typeof createFixture>, countryId: string): void {
    const select = (fixture.nativeElement as HTMLElement).querySelector(
      '#label-country',
    ) as HTMLSelectElement;
    select.value = countryId;
    select.dispatchEvent(new Event('input'));
    select.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function typeInformation(fixture: ReturnType<typeof createFixture>, value: string): void {
    const textarea = (fixture.nativeElement as HTMLElement).querySelector(
      '#label-information',
    ) as HTMLTextAreaElement;
    textarea.value = value;
    textarea.dispatchEvent(new Event('input'));
    textarea.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }

  function submitForm(fixture: ReturnType<typeof createFixture>): void {
    const form = (fixture.nativeElement as HTMLElement).querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit', { cancelable: true }));
  }

  function errorHint(fixture: ReturnType<typeof createFixture>): string | null | undefined {
    return (fixture.nativeElement as HTMLElement).querySelector('.hint.is-error')?.textContent;
  }

  it('zeigt einen Pflichtfeld-Fehler bei leerem Namen nach Blur', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeName(fixture, '');

    // assert
    expect(errorHint(fixture)).toContain('erforderlich');
  });

  it('zeigt einen Fehler bei zu langem Namen', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeName(fixture, 'R'.repeat(61));

    // assert
    expect(errorHint(fixture)).toContain('höchstens 60 Zeichen');
  });

  it('zeigt einen Fehler bei verbotenen Zeichen', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeName(fixture, 'Parlophone (UK)');

    // assert
    expect(errorHint(fixture)).toContain('Buchstaben, Zahlen, Leerzeichen');
  });

  it('zeigt einen Pflichtfeld-Fehler, wenn kein Land ausgewählt ist', () => {
    // arrange
    const fixture = createFixture();

    // act
    selectCountry(fixture, '');

    // assert
    expect(errorHint(fixture)).toContain('Herkunftsland ist erforderlich');
  });

  it('zeigt einen Fehler bei zu langer Information', () => {
    // arrange
    const fixture = createFixture();

    // act
    typeInformation(fixture, 'x'.repeat(256));

    // assert
    expect(errorHint(fixture)).toContain('höchstens 255 Zeichen');
  });

  it('ruft im Anlegen-Modus LabelService.create mit name, countryId und information auf', async () => {
    // arrange
    const created: Label = {
      id: 1,
      name: 'Rough Trade',
      countryId: 2,
      countryName: 'Vereinigtes Königreich',
      information: null,
    };
    labelServiceMock.create.mockReturnValue(of(created));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'Rough Trade');
    selectCountry(fixture, '2');

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(labelServiceMock.create).toHaveBeenCalledWith({
      name: 'Rough Trade',
      countryId: 2,
      information: null,
    });
    expect(savedHandler).toHaveBeenCalledWith(created);
  });

  it('wandelt eine leere Information in null um', async () => {
    // arrange
    labelServiceMock.create.mockReturnValue(
      of({
        id: 1,
        name: 'Rough Trade',
        countryId: 2,
        countryName: 'Vereinigtes Königreich',
        information: null,
      }),
    );
    const fixture = createFixture();
    typeName(fixture, 'Rough Trade');
    selectCountry(fixture, '2');
    typeInformation(fixture, '   ');

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(labelServiceMock.create).toHaveBeenCalledWith(
      expect.objectContaining({ information: null }),
    );
  });

  it('ruft im Bearbeiten-Modus LabelService.update auf und übernimmt bestehende Werte vor', async () => {
    // arrange
    const existing: Label = {
      id: 5,
      name: 'Rough Trade',
      countryId: 2,
      countryName: 'Vereinigtes Königreich',
      information: 'Unabhängiges Label',
    };
    const updated: Label = { ...existing, name: 'Rough Trade Records' };
    labelServiceMock.update.mockReturnValue(of(updated));
    const fixture = createFixture(existing);
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    const countrySelect = (fixture.nativeElement as HTMLElement).querySelector(
      '#label-country',
    ) as HTMLSelectElement;

    // act
    typeName(fixture, 'Rough Trade Records');
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(countrySelect.value).toBe('2');
    expect(labelServiceMock.update).toHaveBeenCalledWith(5, {
      name: 'Rough Trade Records',
      countryId: 2,
      information: 'Unabhängiges Label',
    });
    expect(savedHandler).toHaveBeenCalledWith(updated);
  });

  it('hängt eine 400-Serverantwort für CountryId inline ins Land-Feld ein, ohne das ErrorModal zu öffnen', async () => {
    // arrange
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        errors: { CountryId: ['Das angegebene Land existiert nicht.'] },
        title: 'Validierungsfehler',
        status: 400,
      },
    });
    labelServiceMock.create.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'Rough Trade');
    selectCountry(fixture, '2');

    // act
    submitForm(fixture);
    await fixture.whenStable();
    fixture.detectChanges();

    // assert
    expect(errorHint(fixture)).toContain('Das angegebene Land existiert nicht.');
    expect(errorModalServiceMock.showFromHttpError).not.toHaveBeenCalled();
    expect(savedHandler).not.toHaveBeenCalled();
  });

  it('leitet 409/404/500/Netzwerkfehler an den ErrorModalService weiter, ohne saved zu emittieren', async () => {
    // arrange
    const error = new HttpErrorResponse({ status: 409 });
    labelServiceMock.create.mockReturnValue(throwError(() => error));
    const fixture = createFixture();
    const savedHandler = vi.fn();
    fixture.componentInstance.saved.subscribe(savedHandler);
    typeName(fixture, 'Rough Trade');
    selectCountry(fixture, '2');

    // act
    submitForm(fixture);
    await fixture.whenStable();

    // assert
    expect(errorModalServiceMock.showFromHttpError).toHaveBeenCalledWith(error, 'Label');
    expect(savedHandler).not.toHaveBeenCalled();
  });

  it('hat Tooltips an Abbrechen- und Speichern-Button', () => {
    // arrange
    const fixture = createFixture();
    const compiled = fixture.nativeElement as HTMLElement;

    // act
    const cancelButton = compiled.querySelector('.btn-secondary') as HTMLButtonElement;
    const submitButton = compiled.querySelector('.btn-primary') as HTMLButtonElement;

    // assert
    expect(cancelButton.title).toBe('Abbrechen');
    expect(submitButton.title).toBe('Speichern');
  });

  it('emittiert cancelled beim Abbrechen', () => {
    // arrange
    const fixture = createFixture();
    const cancelledHandler = vi.fn();
    fixture.componentInstance.cancelled.subscribe(cancelledHandler);

    // act
    (
      (fixture.nativeElement as HTMLElement).querySelector('.btn-secondary') as HTMLButtonElement
    ).click();

    // assert
    expect(cancelledHandler).toHaveBeenCalledTimes(1);
  });
});
