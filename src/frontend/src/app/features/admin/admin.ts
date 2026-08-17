import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { switchMap } from 'rxjs';

import { ConfirmModal } from '../../shared/confirm-modal/confirm-modal';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { AdminUser } from './admin-user';
import { AdminUserTable } from './admin-user-table/admin-user-table';
import { AdminService } from './admin.service';

const PAGE_SIZE = 20;

interface AccessTokenClaims {
  sub?: string;
}

@Component({
  selector: 'app-admin',
  imports: [AdminUserTable, ConfirmModal],
  templateUrl: './admin.html',
})
export class Admin {
  private readonly adminService = inject(AdminService);
  private readonly errorModalService = inject(ErrorModalService);
  private readonly oidcSecurityService = inject(OidcSecurityService);

  protected readonly page = signal(1);

  private readonly accessTokenPayload = toSignal(
    toObservable(this.oidcSecurityService.authenticated).pipe(
      switchMap(() => this.oidcSecurityService.getPayloadFromAccessToken()),
    ),
    { initialValue: {} as AccessTokenClaims },
  );

  protected readonly currentUserId = computed(() => this.accessTokenPayload().sub);

  protected readonly usersResource = rxResource({
    params: () => ({ page: this.page(), pageSize: PAGE_SIZE }),
    stream: ({ params }) => this.adminService.getPaged(params.page, params.pageSize),
  });

  protected readonly users = computed(() =>
    this.usersResource.hasValue() ? this.usersResource.value().items : [],
  );
  protected readonly totalPages = computed(() =>
    this.usersResource.hasValue() ? this.usersResource.value().totalPages : 1,
  );
  protected readonly totalCount = computed(() =>
    this.usersResource.hasValue() ? this.usersResource.value().totalCount : 0,
  );

  protected readonly pendingDelete = signal<AdminUser | null>(null);
  protected readonly pendingDeleteMessage = computed(() => {
    const user = this.pendingDelete();
    return user
      ? `Soll der Benutzer „${user.username}" wirklich gelöscht werden? Alle Records, Artists, ` +
          'Labels und Genres dieses Benutzers werden dabei unwiderruflich mitgelöscht.'
      : '';
  });

  constructor() {
    effect(() => {
      const error = this.usersResource.error();

      if (error instanceof HttpErrorResponse) {
        this.errorModalService.showFromHttpError(error, 'Benutzer', () => this.usersResource.reload());
      }
    });
  }

  protected onPageChange(page: number): void {
    this.page.set(page);
  }

  protected onDeleteRequested(user: AdminUser): void {
    this.pendingDelete.set(user);
  }

  protected onDeleteCancelled(): void {
    this.pendingDelete.set(null);
  }

  protected onDeleteConfirmed(): void {
    const user = this.pendingDelete();

    if (!user) {
      return;
    }

    this.adminService.deleteUser(user.id).subscribe({
      next: () => {
        this.pendingDelete.set(null);
        this.usersResource.reload();
      },
      error: (error: HttpErrorResponse) => {
        this.pendingDelete.set(null);
        this.errorModalService.showFromHttpError(error, 'Benutzer');
      },
    });
  }
}
