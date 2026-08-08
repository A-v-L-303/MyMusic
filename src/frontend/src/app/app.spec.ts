import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { KEYCLOAK_EVENT_SIGNAL, KeycloakEventType } from 'keycloak-angular';
import Keycloak from 'keycloak-js';

import { App } from './app';
import { RUNTIME_CONFIG } from './core/runtime-config/runtime-config.service';

describe('App', () => {
  beforeEach(async () => {
    const keycloak = { authenticated: false } as unknown as Keycloak;

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Keycloak, useValue: keycloak },
        {
          provide: KEYCLOAK_EVENT_SIGNAL,
          useValue: signal({ type: KeycloakEventType.KeycloakAngularNotInitialized }),
        },
        {
          provide: RUNTIME_CONFIG,
          useValue: { apiBaseUrl: 'https://localhost:5001', keycloakUrl: 'http://localhost:8080' },
        },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the MyMusic wordmark', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('MyMusic');
  });

  it('zeigt den Login-Button, solange niemand angemeldet ist', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Login');
  });
});
