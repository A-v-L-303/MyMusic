import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, linkedSignal, output } from '@angular/core';
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
import { Artist } from '../artist';
import { ArtistService } from '../artist.service';

interface ArtistFormModel {
  name: string;
}

const NAME_PATTERN = /^[\p{L}\p{N} \-&'./]+$/u;

@Component({
  selector: 'app-artist-form',
  imports: [FormField, Modal],
  templateUrl: './artist-form.html',
})
export class ArtistForm {
  private readonly artistService = inject(ArtistService);
  private readonly errorModalService = inject(ErrorModalService);

  readonly artist = input<Artist | null>(null);

  readonly cancelled = output<void>();
  readonly saved = output<void>();

  protected readonly isEditMode = computed(() => this.artist() !== null);

  protected readonly formModel = linkedSignal<ArtistFormModel>(() => ({
    name: this.artist()?.name ?? '',
  }));
  protected readonly artistForm = form(this.formModel, (path) => {
    required(path.name, { message: 'Der Name ist erforderlich.' });
    minLength(path.name, 3, { message: 'Der Name muss mindestens 3 Zeichen lang sein.' });
    maxLength(path.name, 120, { message: 'Der Name darf höchstens 120 Zeichen lang sein.' });
    pattern(path.name, NAME_PATTERN, {
      message: "Der Name darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / enthalten.",
    });
  });

  protected onCancel(): void {
    this.cancelled.emit();
  }

  protected async onSubmit(): Promise<void> {
    await submit(this.artistForm, (field) => this.save(field));
  }

  private async save(field: FieldTree<ArtistFormModel>): Promise<TreeValidationResult> {
    const name = field().value().name;
    const artist = this.artist();

    try {
      if (artist) {
        await firstValueFrom(this.artistService.update(artist.id, { name }));
      } else {
        await firstValueFrom(this.artistService.create({ name }));
      }

      this.saved.emit();

      return;
    } catch (error) {
      return this.handleSaveError(error, field);
    }
  }

  private handleSaveError(error: unknown, field: FieldTree<ArtistFormModel>): TreeValidationResult {
    if (!(error instanceof HttpErrorResponse)) {
      throw error;
    }

    if (error.status === 400) {
      const nameErrors = (error.error as ValidationProblemDetails | undefined)?.errors?.['Name'];

      if (nameErrors?.length) {
        return { kind: 'server', message: nameErrors[0], fieldTree: field.name };
      }
    }

    this.errorModalService.showFromHttpError(error, 'Artist');

    return;
  }
}
