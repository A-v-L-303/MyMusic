import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { ThemeService } from '../theme.service';
import { ThemeToggle } from './theme-toggle';

describe('ThemeToggle', () => {
  let effectiveThemeSignal: ReturnType<typeof signal<'light' | 'dark'>>;
  let toggleMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    effectiveThemeSignal = signal('light');
    toggleMock = vi.fn();

    TestBed.configureTestingModule({
      imports: [ThemeToggle],
      providers: [
        {
          provide: ThemeService,
          useValue: { effectiveTheme: effectiveThemeSignal, toggle: toggleMock },
        },
      ],
    });
  });

  it('zeigt das Mond-Icon und das passende aria-label, wenn Light aktiv ist', () => {
    // arrange
    const fixture = TestBed.createComponent(ThemeToggle);

    // act
    fixture.detectChanges();

    // assert
    const button = (fixture.nativeElement as HTMLElement).querySelector('button');
    expect(button?.getAttribute('aria-label')).toBe('Zum dunklen Design wechseln');
    expect(button?.title).toBe('Zum dunklen Design wechseln');
    expect(button?.textContent?.trim()).toBe('');
  });

  it('zeigt das Sonnen-Icon und das passende aria-label, wenn Dark aktiv ist', () => {
    // arrange
    effectiveThemeSignal.set('dark');
    const fixture = TestBed.createComponent(ThemeToggle);

    // act
    fixture.detectChanges();

    // assert
    const button = (fixture.nativeElement as HTMLElement).querySelector('button');
    expect(button?.getAttribute('aria-label')).toBe('Zum hellen Design wechseln');
    expect(button?.title).toBe('Zum hellen Design wechseln');
    expect(button?.textContent?.trim()).toBe('');
  });

  it('ruft beim Klick toggle() genau einmal auf', () => {
    // arrange
    const fixture = TestBed.createComponent(ThemeToggle);
    fixture.detectChanges();
    const button = (fixture.nativeElement as HTMLElement).querySelector('button');

    // act
    button?.click();

    // assert
    expect(toggleMock).toHaveBeenCalledTimes(1);
  });
});
