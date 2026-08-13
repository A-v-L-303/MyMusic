import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, output, signal } from '@angular/core';
import {
  FieldTree,
  FormField,
  TreeValidationResult,
  form,
  maxLength,
  minLength,
  pattern,
  required,
  submit,
} from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';

import { Modal } from '../../../shared/modal/modal';
import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { ValidationProblemDetails } from '../../../shared/http/problem-details';
import { Genre } from '../genre';
import { GenreService } from '../genre.service';

interface GenreFormModel {
  name: string;
}

const NAME_PATTERN = /^[\p{L}\p{N} \-&']+$/u;

@Component({
  selector: 'app-genre-form',
  imports: [FormField, Modal],
  templateUrl: './genre-form.html',
})
export class GenreForm {
  private readonly genreService = inject(GenreService);
  private readonly errorModalService = inject(ErrorModalService);

  readonly genre = input<Genre | null>(null);

  readonly cancelled = output<void>();
  readonly saved = output<void>();

  protected readonly isEditMode = computed(() => this.genre() !== null);

  protected readonly formModel = signal<GenreFormModel>({ name: this.genre()?.name ?? '' });
  protected readonly genreForm = form(this.formModel, (path) => {
    required(path.name, { message: 'Der Name ist erforderlich.' });
    minLength(path.name, 3, { message: 'Der Name muss mindestens 3 Zeichen lang sein.' });
    maxLength(path.name, 50, { message: 'Der Name darf höchstens 50 Zeichen lang sein.' });
    pattern(path.name, NAME_PATTERN, {
      message: "Der Name darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' enthalten.",
    });
  });

  protected onCancel(): void {
    this.cancelled.emit();
  }

  protected async onSubmit(): Promise<void> {
    await submit(this.genreForm, (field) => this.save(field));
  }

  private async save(field: FieldTree<GenreFormModel>): Promise<TreeValidationResult> {
    const name = field().value().name;
    const genre = this.genre();

    try {
      if (genre) {
        await firstValueFrom(this.genreService.update(genre.id, { name }));
      } else {
        await firstValueFrom(this.genreService.create({ name }));
      }

      this.saved.emit();

      return;
    } catch (error) {
      return this.handleSaveError(error, field);
    }
  }

  private handleSaveError(error: unknown, field: FieldTree<GenreFormModel>): TreeValidationResult {
    if (!(error instanceof HttpErrorResponse)) {
      throw error;
    }

    if (error.status === 400) {
      const nameErrors = (error.error as ValidationProblemDetails | undefined)?.errors?.['Name'];

      if (nameErrors?.length) {
        return { kind: 'server', message: nameErrors[0], fieldTree: field.name };
      }
    }

    this.errorModalService.showFromHttpError(error, 'Genre');

    return;
  }
}
