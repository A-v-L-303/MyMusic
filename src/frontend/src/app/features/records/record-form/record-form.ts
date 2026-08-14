import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  computed,
  inject,
  input,
  linkedSignal,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { LucidePlus } from '@lucide/angular';
import {
  FieldTree,
  FormField,
  TreeValidationResult,
  form,
  maxLength,
  pattern,
  required,
  submit,
  validate,
} from '@angular/forms/signals';
import { firstValueFrom, of } from 'rxjs';

import { Autocomplete, AutocompleteOption } from '../../../shared/autocomplete/autocomplete';
import { ConfirmModal } from '../../../shared/confirm-modal/confirm-modal';
import { CountryService } from '../../../shared/country/country.service';
import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { ValidationProblemDetails } from '../../../shared/http/problem-details';
import { Modal } from '../../../shared/modal/modal';
import { ArtistService } from '../../artists/artist.service';
import { Label } from '../../labels/label';
import { LabelForm } from '../../labels/label-form/label-form';
import { LabelService } from '../../labels/label.service';
import {
  RECORD_CONDITION_LABELS,
  RECORD_FORMAT_LABELS,
  Record,
  RecordCondition,
  RecordFormat,
} from '../record';
import { RecordService } from '../record.service';

const ALBUM_NAME_PATTERN = /^[\p{L}\p{N} \-&'./()]+$/u;
const MIN_RELEASE_YEAR = 1860;
const SUGGESTION_PAGE_SIZE = 10;
const ARTIST_NAME_PATTERN = /^[\p{L}\p{N} \-&'./]+$/u;
const ARTIST_MIN_NAME_LENGTH = 3;
const ARTIST_MAX_NAME_LENGTH = 120;

interface RecordFormModel {
  labelId: string;
  artistId: string;
  format: string;
  albumName: string;
  releaseYear: string;
  condition: string;
  information: string;
}

@Component({
  selector: 'app-record-form',
  imports: [FormField, Modal, Autocomplete, ConfirmModal, LabelForm, LucidePlus],
  templateUrl: './record-form.html',
})
export class RecordForm {
  private readonly recordService = inject(RecordService);
  private readonly labelService = inject(LabelService);
  private readonly artistService = inject(ArtistService);
  private readonly countryService = inject(CountryService);
  private readonly errorModalService = inject(ErrorModalService);

  protected readonly labelAutocomplete = viewChild<Autocomplete>('labelAutocomplete');
  protected readonly artistAutocomplete = viewChild<Autocomplete>('artistAutocomplete');

  readonly record = input<Record | null>(null);

  readonly cancelled = output<void>();
  readonly saved = output<void>();

  protected readonly isEditMode = computed(() => this.record() !== null);
  protected readonly currentYear = new Date().getFullYear();

  protected readonly formatOptions = Object.entries(RECORD_FORMAT_LABELS) as [
    RecordFormat,
    string,
  ][];
  protected readonly conditionOptions = Object.entries(RECORD_CONDITION_LABELS) as [
    RecordCondition,
    string,
  ][];

  protected readonly labelQuery = signal('');
  protected readonly artistQuery = signal('');
  protected readonly attemptedSubmit = signal(false);

  protected readonly labelSuggestionsResource = rxResource({
    params: () => ({ query: this.labelQuery() }),
    stream: ({ params }) =>
      params.query
        ? this.labelService.getPaged(1, SUGGESTION_PAGE_SIZE, params.query)
        : of({ items: [], totalCount: 0, page: 1, pageSize: SUGGESTION_PAGE_SIZE, totalPages: 0 }),
  });

  protected readonly artistSuggestionsResource = rxResource({
    params: () => ({ query: this.artistQuery() }),
    stream: ({ params }) =>
      params.query
        ? this.artistService.getPaged(1, SUGGESTION_PAGE_SIZE, params.query)
        : of({ items: [], totalCount: 0, page: 1, pageSize: SUGGESTION_PAGE_SIZE, totalPages: 0 }),
  });

  protected readonly countriesResource = rxResource({
    stream: () => this.countryService.getAll(),
  });
  protected readonly countries = computed(() =>
    this.countriesResource.hasValue() ? this.countriesResource.value() : [],
  );

  protected readonly artistDisplayName = linkedSignal(() => this.record()?.artistName ?? '');
  protected readonly labelCreateOpen = signal(false);
  protected readonly pendingNewArtistName = signal<string | null>(null);

  protected readonly labelSuggestions = computed<AutocompleteOption[]>(() =>
    this.labelSuggestionsResource.hasValue()
      ? this.labelSuggestionsResource.value().items.map((label) => ({
          id: label.id,
          label: label.name,
        }))
      : [],
  );
  protected readonly artistSuggestions = computed<AutocompleteOption[]>(() =>
    this.artistSuggestionsResource.hasValue()
      ? this.artistSuggestionsResource.value().items.map((artist) => ({
          id: artist.id,
          label: artist.name,
        }))
      : [],
  );

  protected readonly formModel = linkedSignal(() => this.buildInitialModel());
  protected readonly recordForm = form(this.formModel, (path) => {
    required(path.labelId, { message: 'Das Label ist erforderlich.' });
    required(path.format, { message: 'Das Format ist erforderlich.' });
    required(path.albumName, { message: 'Der Albumname ist erforderlich.' });
    maxLength(path.albumName, 150, {
      message: 'Der Albumname darf höchstens 150 Zeichen lang sein.',
    });
    pattern(path.albumName, ALBUM_NAME_PATTERN, {
      message:
        "Der Albumname darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / ( ) enthalten.",
    });
    required(path.releaseYear, { message: 'Das Erscheinungsjahr ist erforderlich.' });
    validate(path.releaseYear, ({ value }) => {
      const raw = value().trim();

      if (!raw) {
        return undefined;
      }

      const year = Number(raw);

      if (!Number.isInteger(year) || year < MIN_RELEASE_YEAR || year > this.currentYear) {
        return {
          kind: 'custom',
          message: `Das Erscheinungsjahr muss zwischen ${MIN_RELEASE_YEAR} und ${this.currentYear} liegen.`,
        };
      }

      return undefined;
    });
    maxLength(path.information, 255, {
      message: "Das Feld 'information' darf höchstens 255 Zeichen lang sein.",
    });
  });

  private buildInitialModel(): RecordFormModel {
    const record = this.record();

    return {
      labelId: record ? String(record.labelId) : '',
      artistId: record?.artistId ? String(record.artistId) : '',
      format: record?.format ?? '',
      albumName: record?.albumName ?? '',
      releaseYear: record ? String(record.releaseYear) : '',
      condition: record?.condition ?? 'Vg',
      information: record?.information ?? '',
    };
  }

  protected onLabelQueryChange(query: string): void {
    this.labelQuery.set(query);
  }

  protected onArtistQueryChange(query: string): void {
    this.artistQuery.set(query);
  }

  protected onLabelSelected(option: AutocompleteOption | undefined): void {
    this.formModel.update((model) => ({ ...model, labelId: option ? String(option.id) : '' }));
  }

  protected onArtistSelected(option: AutocompleteOption | undefined): void {
    this.formModel.update((model) => ({ ...model, artistId: option ? String(option.id) : '' }));
    this.artistDisplayName.set(option?.label ?? '');
  }

  protected onArtistBlur(text: string): void {
    const trimmed = text.trim();

    if (!trimmed || trimmed === this.artistDisplayName()) {
      return;
    }

    if (
      trimmed.length < ARTIST_MIN_NAME_LENGTH ||
      trimmed.length > ARTIST_MAX_NAME_LENGTH ||
      !ARTIST_NAME_PATTERN.test(trimmed)
    ) {
      return;
    }

    this.pendingNewArtistName.set(trimmed);
  }

  protected async onCreateArtistConfirmed(): Promise<void> {
    const name = this.pendingNewArtistName();
    this.pendingNewArtistName.set(null);

    if (!name) {
      return;
    }

    try {
      const artist = await firstValueFrom(this.artistService.create({ name }));

      this.formModel.update((model) => ({ ...model, artistId: String(artist.id) }));
      this.artistDisplayName.set(artist.name);
      this.artistAutocomplete()?.setQuery(artist.name);
    } catch (error) {
      if (!(error instanceof HttpErrorResponse)) {
        throw error;
      }

      this.errorModalService.showFromHttpError(error, 'Künstler');
    }
  }

  protected onCreateArtistCancelled(): void {
    this.pendingNewArtistName.set(null);
    this.formModel.update((model) => ({ ...model, artistId: '' }));
    this.artistDisplayName.set('');
    this.artistAutocomplete()?.setQuery('');
  }

  protected openLabelCreate(): void {
    this.labelCreateOpen.set(true);
  }

  protected onLabelCreateCancelled(): void {
    this.labelCreateOpen.set(false);
  }

  protected onLabelCreateSaved(label: Label): void {
    this.formModel.update((model) => ({ ...model, labelId: String(label.id) }));
    this.labelAutocomplete()?.setQuery(label.name);
    this.labelCreateOpen.set(false);
  }

  protected onCancel(): void {
    this.cancelled.emit();
  }

  protected async onSubmit(): Promise<void> {
    this.attemptedSubmit.set(true);
    await submit(this.recordForm, (field) => this.save(field));
  }

  private async save(field: FieldTree<RecordFormModel>): Promise<TreeValidationResult> {
    const value = field().value();
    const request = {
      labelId: Number(value.labelId),
      artistId: value.artistId ? Number(value.artistId) : null,
      format: value.format as RecordFormat,
      albumName: value.albumName,
      releaseYear: Number(value.releaseYear),
      condition: value.condition as RecordCondition,
      information: value.information.trim() ? value.information : null,
    };
    const record = this.record();

    try {
      if (record) {
        await firstValueFrom(this.recordService.update(record.id, request));
      } else {
        await firstValueFrom(this.recordService.create(request));
      }

      this.saved.emit();

      return;
    } catch (error) {
      return this.handleSaveError(error, field);
    }
  }

  private handleSaveError(error: unknown, field: FieldTree<RecordFormModel>): TreeValidationResult {
    if (!(error instanceof HttpErrorResponse)) {
      throw error;
    }

    if (error.status === 400) {
      const errors = (error.error as ValidationProblemDetails | undefined)?.errors;
      const fieldMap: [string, FieldTree<string>][] = [
        ['LabelId', field.labelId],
        ['ArtistId', field.artistId],
        ['Format', field.format],
        ['AlbumName', field.albumName],
        ['ReleaseYear', field.releaseYear],
        ['Condition', field.condition],
        ['Information', field.information],
      ];

      for (const [key, fieldTree] of fieldMap) {
        const fieldErrors = errors?.[key];

        if (fieldErrors?.length) {
          return { kind: 'server', message: fieldErrors[0], fieldTree };
        }
      }
    }

    this.errorModalService.showFromHttpError(error, 'Record');

    return;
  }
}
