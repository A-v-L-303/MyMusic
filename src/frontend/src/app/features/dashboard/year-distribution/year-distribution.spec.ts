import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { YearCount } from '../dashboard-stats';
import { YearDistribution } from './year-distribution';

describe('YearDistribution', () => {
  it('zeigt eine Textmeldung, wenn keine Jahre vorhanden sind', () => {
    // arrange
    const fixture = TestBed.createComponent(YearDistribution);
    fixture.componentRef.setInput('items', []);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Keine Daten vorhanden.');
  });

  it('rendert einen Balken für jedes Jahr zwischen dem ersten und letzten vorhandenen Jahr, auch für Luecken', () => {
    // arrange
    const items: YearCount[] = [
      { year: 1980, count: 2 },
      { year: 1985, count: 5 },
    ];
    const fixture = TestBed.createComponent(YearDistribution);
    fixture.componentRef.setInput('items', items);

    // act
    fixture.detectChanges();

    // assert: 1980..1985 sind sechs Jahre, auch die Luecken ohne Records bekommen einen (leeren) Balken
    const bars = (fixture.nativeElement as HTMLElement).querySelectorAll('[title]');
    expect(bars.length).toBe(6);
    expect(bars[0].getAttribute('title')).toBe('1980: 2');
    expect(bars[3].getAttribute('title')).toBe('1983: 0');
    expect(bars[5].getAttribute('title')).toBe('1985: 5');
  });

  it('zeigt das erste und letzte Jahr immer als sichtbares Label an', () => {
    // arrange
    const items: YearCount[] = [
      { year: 1980, count: 2 },
      { year: 1985, count: 5 },
    ];
    const fixture = TestBed.createComponent(YearDistribution);
    fixture.componentRef.setInput('items', items);

    // act
    fixture.detectChanges();

    // assert
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('1980');
    expect(compiled.textContent).toContain('1985');
  });

  it('zeigt die Anzahl sichtbar oberhalb jedes beschrifteten Balkens an', () => {
    // arrange
    const items: YearCount[] = [
      { year: 1980, count: 2 },
      { year: 1985, count: 5 },
    ];
    const fixture = TestBed.createComponent(YearDistribution);
    fixture.componentRef.setInput('items', items);

    // act
    fixture.detectChanges();

    // assert: bei nur sechs Jahren (1980..1985) wird bei jedem Jahr beschriftet, auch bei den
    // Luecken ohne Records (Anzahl 0)
    const countLabels = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll('.tabular-nums'),
    ).map((element) => element.textContent?.trim());
    expect(countLabels).toEqual(['2', '0', '0', '0', '0', '5']);
  });

  it('duennt Jahres- und Anzahl-Beschriftungen bei einer grossen Jahresspanne aus', () => {
    // arrange
    const items: YearCount[] = [
      { year: 1950, count: 1 },
      { year: 2000, count: 3 },
    ];
    const fixture = TestBed.createComponent(YearDistribution);
    fixture.componentRef.setInput('items', items);

    // act
    fixture.detectChanges();

    // assert: 51 Jahre (1950..2000) ergeben 51 Balken, aber nicht bei jedem ein sichtbares Label
    const compiled = fixture.nativeElement as HTMLElement;
    const yearBars = compiled.querySelectorAll('[title]');
    const countLabels = compiled.querySelectorAll('.tabular-nums');
    expect(yearBars.length).toBe(51);
    expect(countLabels.length).toBeGreaterThan(0);
    expect(countLabels.length).toBeLessThan(51);
  });
});
