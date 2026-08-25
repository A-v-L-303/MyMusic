import { Component, ElementRef, HostListener, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormField, form, minLength, pattern } from '@angular/forms/signals';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import {
  LucideChevronDown,
  LucideDisc3,
  LucideLayoutDashboard,
  LucideMusic,
  LucideSearch,
  LucideTag,
  LucideUsers,
} from '@lucide/angular';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { filter, map } from 'rxjs';

import { UserRolesService } from '../core/auth/user-roles.service';
import { ThemeToggle } from '../core/theme/theme-toggle/theme-toggle';

interface OidcUserClaims {
  preferred_username?: string;
}

const SEARCH_QUERY_MIN_LENGTH = 2;

const SEARCH_QUERY_PATTERN = /^[\p{L}\p{N} \-&'./()]+$/u;

@Component({
  selector: 'app-nav',
  imports: [
    RouterLink,
    RouterLinkActive,
    FormField,
    ThemeToggle,
    LucideLayoutDashboard,
    LucideDisc3,
    LucideUsers,
    LucideTag,
    LucideMusic,
    LucideSearch,
    LucideChevronDown,
  ],
  templateUrl: './nav.html',
})
export class Nav {
  private readonly elementRef = inject(ElementRef<HTMLElement>);
  private readonly router = inject(Router);
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private readonly userRolesService = inject(UserRolesService);

  protected readonly authenticated = this.oidcSecurityService.authenticated;
  protected readonly isAdmin = this.userRolesService.isAdmin;
  protected readonly username = computed(() => {
    const claims = this.oidcSecurityService.userData().userData as OidcUserClaims | undefined;
    return claims?.preferred_username;
  });

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map(() => this.router.url),
    ),
    { initialValue: this.router.url },
  );

  protected readonly optionMenuOpen = signal(false);
  protected readonly optionMenuActive = computed(() => {
    const url = this.currentUrl();
    return url.startsWith('/artists') || url.startsWith('/labels') || url.startsWith('/genres');
  });

  protected readonly searchModel = signal({ query: '' });
  protected readonly searchForm = form(this.searchModel, (path) => {
    minLength(path.query, SEARCH_QUERY_MIN_LENGTH, {
      message: `Die Eingabe muss mindestens ${SEARCH_QUERY_MIN_LENGTH} Zeichen lang sein.`,
    });
    pattern(path.query, SEARCH_QUERY_PATTERN, {
      message:
        "Die Eingabe darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / ( ) enthalten.",
    });
  });

  protected readonly attemptedSubmit = signal(false);

  protected login(): void {
    this.oidcSecurityService.authorize();
  }

  protected register(): void {
    this.oidcSecurityService.authorize(undefined, {
      urlHandler: (url) => {
        window.location.href = url.replace(
          '/protocol/openid-connect/auth',
          '/protocol/openid-connect/registrations',
        );
      },
    });
  }

  protected logout(): void {
    this.oidcSecurityService.logoff().subscribe();
  }

  protected toggleOptionMenu(): void {
    this.optionMenuOpen.update((open) => !open);
  }

  protected closeOptionMenu(): void {
    this.optionMenuOpen.set(false);
  }

  protected submitSearch(): void {
    this.attemptedSubmit.set(true);
    const query = this.searchModel().query.trim();
    if (!query) {
      return;
    }
    if (this.searchForm.query().invalid()) {
      return;
    }
    this.router.navigate(['/search'], { queryParams: { q: query } });
  }

  @HostListener('document:click', ['$event'])
  protected onDocumentClick(event: MouseEvent): void {
    if (!this.optionMenuOpen()) {
      return;
    }
    if (!this.elementRef.nativeElement.contains(event.target as Node)) {
      this.closeOptionMenu();
    }
  }
}
