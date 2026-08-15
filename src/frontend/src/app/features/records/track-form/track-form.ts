import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, linkedSignal, output, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
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

import { ArtistService } from '../../artists/artist.service';
import { GenreService } from '../../genres/genre.service';
import { Autocomplete, AutocompleteOption } from '../../../shared/autocomplete/autocomplete';
import { ErrorModalService } from '../../../shared/error-modal/error-modal.service';
import { ValidationProblemDetails } from '../../../shared/http/problem-details';
import { Modal } from '../../../shared/modal/modal';
import { CreateTrackRequest, RecordTrack } from '../record';
import { RecordService } from '../record.service';

const TRACK_NAME_PATTERN = /^[\p{L}\p{N} \-&'./()]+$/u;
const RECORD_SIDE_PATTERN = /^[\p{L}\p{N}]{1,3}$/u;
const SUGGESTION_PAGE_SIZE = 10;

interface TrackFormModel {
  artistId: string;
  genreId: string;
  trackName: string;
  recordSide: string;
  trackNumber: string;
  information: string;
}

@Component({
  selector: 'app-track-form',
  imports: [FormField, Modal, Autocomplete],
  templateUrl: './track-form.html',
})
export class TrackForm {
  private readonly recordService = inject(RecordService);
  private readonly artistService = inject(ArtistService);
  private readonly genreService = inject(GenreService);
  private readonly errorModalService = inject(ErrorModalService);

  readonly track = input<RecordTrack | null>(null);
  readonly recordId = input.required<number>();

  readonly cancelled = output<void>();
  readonly saved = output<void>();

  protected readonly isEditMode = computed(() => this.track() !== null);
  protected readonly attemptedSubmit = signal(false);

  protected readonly artistQuery = signal('');

  protected readonly artistSuggestionsResource = rxResource({
    params: () => ({ query: this.artistQuery() }),
    stream: ({ params }) =>
      params.query
        ? this.artistService.getPaged(1, SUGGESTION_PAGE_SIZE, params.query)
        : of({ items: [], totalCount: 0, page: 1, pageSize: SUGGESTION_PAGE_SIZE, totalPages: 0 }),
  });

  protected readonly artistSuggestions = computed<AutocompleteOption[]>(() =>
    this.artistSuggestionsResource.hasValue()
      ? this.artistSuggestionsResource.value().items.map((artist) => ({
          id: artist.id,
          label: artist.name,
        }))
      : [],
  );

  protected readonly genresResource = rxResource({
    stream: () => this.genreService.getAll(),
  });
  protected readonly genres = computed(() =>
    this.genresResource.hasValue() ? this.genresResource.value() : [],
  );

  protected readonly formModel = linkedSignal(() => this.buildInitialModel());
  protected readonly trackForm = form(this.formModel, (path) => {
    required(path.artistId, { message: 'Der Künstler ist erforderlich.' });
    required(path.genreId, { message: 'Das Genre ist erforderlich.' });
    required(path.trackName, { message: 'Der Trackname ist erforderlich.' });
    maxLength(path.trackName, 150, {
      message: 'Der Trackname darf höchstens 150 Zeichen lang sein.',
    });
    pattern(path.trackName, TRACK_NAME_PATTERN, {
      message:
        "Der Trackname darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / ( ) enthalten.",
    });
    maxLength(path.recordSide, 3, { message: 'Die Seite darf höchstens 3 Zeichen lang sein.' });
    pattern(path.recordSide, RECORD_SIDE_PATTERN, {
      message: 'Die Seite darf nur Buchstaben oder Ziffern enthalten.',
    });
    required(path.trackNumber, { message: 'Die Tracknummer ist erforderlich.' });
    validate(path.trackNumber, ({ value }) => {
      const raw = value().trim();

      if (!raw) {
        return undefined;
      }

      const trackNumber = Number(raw);

      if (!Number.isInteger(trackNumber) || trackNumber < 1) {
        return { kind: 'custom', message: 'Die Tracknummer muss mindestens 1 sein.' };
      }

      return undefined;
    });
    maxLength(path.information, 255, {
      message: "Das Feld 'information' darf höchstens 255 Zeichen lang sein.",
    });
  });

  private buildInitialModel(): TrackFormModel {
    const track = this.track();

    return {
      artistId: track ? String(track.artistId) : '',
      genreId: track ? String(track.genreId) : '',
      trackName: track?.trackName ?? '',
      recordSide: track?.recordSide ?? '0',
      trackNumber: track ? String(track.trackNumber) : '',
      information: track?.information ?? '',
    };
  }

  protected onArtistQueryChange(query: string): void {
    this.artistQuery.set(query);
  }

  protected onArtistSelected(option: AutocompleteOption | undefined): void {
    this.formModel.update((model) => ({ ...model, artistId: option ? String(option.id) : '' }));
  }

  protected onCancel(): void {
    this.cancelled.emit();
  }

  protected async onSubmit(): Promise<void> {
    this.attemptedSubmit.set(true);
    await submit(this.trackForm, (field) => this.save(field));
  }

  private async save(field: FieldTree<TrackFormModel>): Promise<TreeValidationResult> {
    const value = field().value();
    const request: CreateTrackRequest = {
      artistId: Number(value.artistId),
      genreId: Number(value.genreId),
      trackName: value.trackName,
      recordSide: value.recordSide.trim() ? value.recordSide : '0',
      trackNumber: Number(value.trackNumber),
      information: value.information.trim() ? value.information : null,
    };
    const track = this.track();

    try {
      if (track) {
        await firstValueFrom(this.recordService.updateTrack(this.recordId(), track.id, request));
      } else {
        await firstValueFrom(this.recordService.createTrack(this.recordId(), request));
      }

      this.saved.emit();

      return;
    } catch (error) {
      return this.handleSaveError(error, field);
    }
  }

  private handleSaveError(error: unknown, field: FieldTree<TrackFormModel>): TreeValidationResult {
    if (!(error instanceof HttpErrorResponse)) {
      throw error;
    }

    if (error.status === 400) {
      const errors = (error.error as ValidationProblemDetails | undefined)?.errors;
      const fieldMap: [string, FieldTree<string>][] = [
        ['ArtistId', field.artistId],
        ['GenreId', field.genreId],
        ['TrackName', field.trackName],
        ['RecordSide', field.recordSide],
        ['TrackNumber', field.trackNumber],
        ['Information', field.information],
      ];

      for (const [key, fieldTree] of fieldMap) {
        const fieldErrors = errors?.[key];

        if (fieldErrors?.length) {
          return { kind: 'server', message: fieldErrors[0], fieldTree };
        }
      }
    }

    this.errorModalService.showFromHttpError(error, 'Track');

    return;
  }
}
