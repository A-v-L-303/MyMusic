import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, linkedSignal, output } from '@angular/core';
import {
  FieldTree,
  FormField,
  TreeValidationResult,
  form,
  maxLength,
  required,
  pattern,
  submit,
} from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';

import { Country } from '../../../shared/country/country';
import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { ValidationProblemDetails } from '../../../shared/http/problem-details';
import { Modal } from '../../../shared/modal/modal';
import { Label } from '../label';
import { LabelService } from '../label.service';

interface LabelFormModel {
  name: string;
  countryId: string;
  information: string;
}

const NAME_PATTERN = /^[\p{L}\p{N} \-&'./]+$/u;

@Component({
  selector: 'app-label-form',
  imports: [FormField, Modal],
  templateUrl: './label-form.html',
})
export class LabelForm {
  private readonly labelService = inject(LabelService);
  private readonly errorModalService = inject(ErrorModalService);

  readonly label = input<Label | null>(null);
  readonly countries = input.required<Country[]>();

  readonly cancelled = output<void>();
  readonly saved = output<Label>();

  protected readonly isEditMode = computed(() => this.label() !== null);

  protected readonly formModel = linkedSignal(() => this.buildInitialModel());
  protected readonly labelForm = form(this.formModel, (path) => {
    required(path.name, { message: 'Der Name ist erforderlich.' });
    maxLength(path.name, 60, { message: 'Der Name darf höchstens 60 Zeichen lang sein.' });
    pattern(path.name, NAME_PATTERN, {
      message: "Der Name darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / enthalten.",
    });
    required(path.countryId, { message: 'Das Herkunftsland ist erforderlich.' });
    maxLength(path.information, 255, {
      message: "Das Feld 'information' darf höchstens 255 Zeichen lang sein.",
    });
  });

  private buildInitialModel(): LabelFormModel {
    const label = this.label();

    return {
      name: label?.name ?? '',
      countryId: label ? String(label.countryId) : '',
      information: label?.information ?? '',
    };
  }

  protected onCancel(): void {
    this.cancelled.emit();
  }

  protected async onSubmit(): Promise<void> {
    await submit(this.labelForm, (field) => this.save(field));
  }

  private async save(field: FieldTree<LabelFormModel>): Promise<TreeValidationResult> {
    const value = field().value();
    const request = {
      name: value.name,
      countryId: Number(value.countryId),
      information: value.information.trim() ? value.information : null,
    };
    const label = this.label();

    try {
      const saved = label
        ? await firstValueFrom(this.labelService.update(label.id, request))
        : await firstValueFrom(this.labelService.create(request));

      this.saved.emit(saved);

      return;
    } catch (error) {
      return this.handleSaveError(error, field);
    }
  }

  private handleSaveError(error: unknown, field: FieldTree<LabelFormModel>): TreeValidationResult {
    if (!(error instanceof HttpErrorResponse)) {
      throw error;
    }

    if (error.status === 400) {
      const errors = (error.error as ValidationProblemDetails | undefined)?.errors;
      const nameErrors = errors?.['Name'];
      const countryErrors = errors?.['CountryId'];
      const informationErrors = errors?.['Information'];

      if (nameErrors?.length) {
        return { kind: 'server', message: nameErrors[0], fieldTree: field.name };
      }

      if (countryErrors?.length) {
        return { kind: 'server', message: countryErrors[0], fieldTree: field.countryId };
      }

      if (informationErrors?.length) {
        return { kind: 'server', message: informationErrors[0], fieldTree: field.information };
      }
    }

    this.errorModalService.showFromHttpError(error, 'Label');

    return;
  }
}
