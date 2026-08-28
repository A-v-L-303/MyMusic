import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, linkedSignal, output, signal } from '@angular/core';
import {
  FieldTree,
  FormField,
  TreeValidationResult,
  email,
  form,
  maxLength,
  minLength,
  required,
  submit,
  validate,
} from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';

import { Modal } from '../../shared/modal/modal';
import { ErrorModalService } from '../../shared/error-modal/error-modal.service';
import { ValidationProblemDetails } from '../../shared/http/problem-details';
import { UserProfileService } from './user-profile.service';

interface EmailFormModel {
  email: string;
}

interface PasswordFormModel {
  newPassword: string;
  newPasswordConfirmation: string;
}

const EMAIL_MAX_LENGTH = 255;

const PASSWORD_MIN_LENGTH = 8;

const PASSWORD_MAX_LENGTH = 100;

const PLACEHOLDER_HINT = ' ';

@Component({
  selector: 'app-user-profile',
  imports: [FormField, Modal],
  templateUrl: './user-profile.html',
})
export class UserProfile {
  private readonly userProfileService = inject(UserProfileService);
  private readonly errorModalService = inject(ErrorModalService);

  readonly username = input.required<string | undefined>();
  readonly email = input.required<string | undefined>();

  readonly closed = output<void>();
  readonly emailChanged = output<string>();

  protected readonly placeholderHint = PLACEHOLDER_HINT;

  protected readonly emailFormModel = linkedSignal<EmailFormModel>(() => ({ email: this.email() ?? '' }));
  protected readonly emailForm = form(this.emailFormModel, (path) => {
    required(path.email, { message: 'Die E-Mail-Adresse ist erforderlich.' });
    maxLength(path.email, EMAIL_MAX_LENGTH, {
      message: `Die E-Mail-Adresse darf höchstens ${EMAIL_MAX_LENGTH} Zeichen lang sein.`,
    });
    email(path.email, { message: 'Die E-Mail-Adresse hat kein gültiges Format.' });
  });

  protected readonly emailSavedMessage = signal<string | null>(null);

  protected readonly emailHintIsError = computed(
    () => this.emailForm.email().invalid() && this.emailForm.email().touched(),
  );
  protected readonly emailHintText = computed(() => {
    if (this.emailHintIsError()) {
      return this.emailForm.email().errors()[0].message;
    }
    return this.emailSavedMessage() ?? '';
  });

  protected readonly passwordFormModel = signal<PasswordFormModel>({
    newPassword: '',
    newPasswordConfirmation: '',
  });
  protected readonly passwordForm = form(this.passwordFormModel, (path) => {
    required(path.newPassword, { message: 'Das Passwort ist erforderlich.' });
    minLength(path.newPassword, PASSWORD_MIN_LENGTH, {
      message: `Das Passwort muss mindestens ${PASSWORD_MIN_LENGTH} Zeichen lang sein.`,
    });
    maxLength(path.newPassword, PASSWORD_MAX_LENGTH, {
      message: `Das Passwort darf höchstens ${PASSWORD_MAX_LENGTH} Zeichen lang sein.`,
    });
    required(path.newPasswordConfirmation, { message: 'Die Wiederholung ist erforderlich.' });
    validate(path.newPasswordConfirmation, (ctx) => {
      if (ctx.value() !== ctx.valueOf(path.newPassword)) {
        return { kind: 'mismatch', message: 'Die Passwörter stimmen nicht überein.' };
      }
      return undefined;
    });
  });

  protected readonly passwordSavedMessage = signal<string | null>(null);

  protected readonly newPasswordHintText = computed(() => {
    const field = this.passwordForm.newPassword();
    return field.invalid() && field.touched() ? field.errors()[0].message : '';
  });

  protected readonly newPasswordConfirmationHintIsError = computed(() => {
    const field = this.passwordForm.newPasswordConfirmation();
    return field.invalid() && field.touched();
  });
  protected readonly newPasswordConfirmationHintText = computed(() => {
    if (this.newPasswordConfirmationHintIsError()) {
      return this.passwordForm.newPasswordConfirmation().errors()[0].message;
    }
    return this.passwordSavedMessage() ?? '';
  });

  protected onClose(): void {
    this.closed.emit();
  }

  protected async onSubmitEmail(): Promise<void> {
    this.emailSavedMessage.set(null);
    await submit(this.emailForm, (field) => this.saveEmail(field));
  }

  protected async onSubmitPassword(): Promise<void> {
    this.passwordSavedMessage.set(null);
    await submit(this.passwordForm, (field) => this.savePassword(field));
  }

  private async saveEmail(field: FieldTree<EmailFormModel>): Promise<TreeValidationResult> {
    const value = field().value().email;

    try {
      await firstValueFrom(this.userProfileService.updateEmail(value));

      this.emailSavedMessage.set('E-Mail-Adresse wurde geändert.');
      this.emailChanged.emit(value);

      return;
    } catch (error) {
      return this.handleSaveError(error, field.email, 'Email', 'E-Mail-Adresse');
    }
  }

  private async savePassword(field: FieldTree<PasswordFormModel>): Promise<TreeValidationResult> {
    const value = field().value().newPassword;

    try {
      await firstValueFrom(this.userProfileService.changePassword(value));

      field().reset({ newPassword: '', newPasswordConfirmation: '' });
      this.passwordSavedMessage.set('Passwort wurde geändert.');

      return;
    } catch (error) {
      return this.handleSaveError(error, field.newPassword, 'NewPassword', 'Passwort');
    }
  }

  private handleSaveError(
    error: unknown,
    fieldTree: FieldTree<string>,
    validationPropertyName: string,
    entityName: string,
  ): TreeValidationResult {
    if (!(error instanceof HttpErrorResponse)) {
      throw error;
    }

    if (error.status === 400) {
      const fieldErrors = (error.error as ValidationProblemDetails | undefined)?.errors?.[
        validationPropertyName
      ];

      if (fieldErrors?.length) {
        return { kind: 'server', message: fieldErrors[0], fieldTree };
      }
    }

    this.errorModalService.showFromHttpError(error, entityName);

    return;
  }
}
