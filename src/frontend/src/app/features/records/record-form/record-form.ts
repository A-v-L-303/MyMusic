import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  OnDestroy,
  computed,
  inject,
  input,
  isDevMode,
  linkedSignal,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { LucideDisc3, LucidePlus } from '@lucide/angular';
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
import { GenreService } from '../../genres/genre.service';
import { Label } from '../../labels/label';
import { LabelForm } from '../../labels/label-form/label-form';
import { LabelService } from '../../labels/label.service';
import { DiscogsRelease, DiscogsTrack } from '../discogs';
import { dataUrlToFile } from '../discogs-cover';
import {
  sanitizeDiscogsArtistName,
  sanitizeDiscogsGenreName,
  sanitizeDiscogsLabelName,
} from '../discogs-name-sanitizer';
import { parseDiscogsPosition } from '../discogs-position';
import { DiscogsSearch } from '../discogs-search/discogs-search';
import {
  ALLOWED_ALBUM_COVER_CONTENT_TYPES,
  MAX_ALBUM_COVER_SIZE_BYTES,
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
const GENRE_MIN_NAME_LENGTH = 3;

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
  imports: [
    FormField,
    Modal,
    Autocomplete,
    ConfirmModal,
    LabelForm,
    DiscogsSearch,
    LucidePlus,
    LucideDisc3,
  ],
  templateUrl: './record-form.html',
})
export class RecordForm implements OnDestroy {
  private readonly recordService = inject(RecordService);
  private readonly labelService = inject(LabelService);
  private readonly artistService = inject(ArtistService);
  private readonly genreService = inject(GenreService);
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

  protected readonly selectedCoverFile = signal<File | null>(null);
  protected readonly previewUrl = linkedSignal(() => this.record()?.albumCoverDataUrl ?? null);

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

  // Für den Discogs-Existenz-Abgleich (US-DI3): unpaginierte Gesamtlisten je Referenztyp.
  protected readonly artistsResource = rxResource({ stream: () => this.artistService.getAll() });
  protected readonly labelsResource = rxResource({ stream: () => this.labelService.getAll() });
  protected readonly genresResource = rxResource({ stream: () => this.genreService.getAll() });
  private readonly artists = computed(() =>
    this.artistsResource.hasValue() ? this.artistsResource.value() : [],
  );
  private readonly labels = computed(() =>
    this.labelsResource.hasValue() ? this.labelsResource.value() : [],
  );
  private readonly genres = computed(() =>
    this.genresResource.hasValue() ? this.genresResource.value() : [],
  );

  protected readonly artistDisplayName = linkedSignal(() => this.record()?.artistName ?? '');
  protected readonly labelCreateOpen = signal(false);
  protected readonly discogsLabelPrefillName = signal('');
  protected readonly pendingArtistConfirmName = signal<string | null>(null);
  protected readonly pendingGenreConfirmName = signal<string | null>(null);

  protected readonly discogsSearchOpen = signal(false);
  private readonly discogsTracklist = signal<DiscogsTrack[]>([]);
  private readonly discogsRecordArtistName = signal<string | null>(null);
  private readonly discogsResolvedGenreId = signal<number | null>(null);

  private pendingArtistResolve: ((id: number | null) => void) | null = null;
  private pendingGenreResolve: ((id: number | null) => void) | null = null;
  private pendingLabelResolve: ((id: number | null) => void) | null = null;

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

  protected async onArtistBlur(text: string): Promise<void> {
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

    const id = await this.resolveArtistId(trimmed);

    if (id) {
      this.formModel.update((model) => ({ ...model, artistId: String(id) }));
      this.artistDisplayName.set(trimmed);
      this.artistAutocomplete()?.setQuery(trimmed);
    } else {
      this.formModel.update((model) => ({ ...model, artistId: '' }));
      this.artistDisplayName.set('');
      this.artistAutocomplete()?.setQuery('');
    }
  }

  /**
   * Löst einen Artist-Namen zu einer Id auf: exakter Treffer in `artists()` wird direkt
   * referenziert, sonst öffnet sich eine Rückfrage zur Neuanlage (US-DI3). Wird sowohl für
   * den Record-Artist als auch für jeden Track-Artist beim Discogs-Import wiederverwendet.
   *
   * Discogs-Namen können Zeichen enthalten, die `ARTIST_NAME_PATTERN` nicht erlaubt (z. B.
   * Disambiguierungs-Suffixe wie „ (2)", Kommas, Anführungszeichen) — der Name wird deshalb
   * vor dem Existenz-Abgleich und der Neuanlage über {@link sanitizeDiscogsArtistName}
   * bereinigt. Manuell über die Autocomplete eingegebene Namen sind bereits konform und
   * bleiben durch die Bereinigung unverändert.
   */
  private resolveArtistId(name: string): Promise<number | null> {
    const cleanedName = sanitizeDiscogsArtistName(name);

    if (cleanedName.length < ARTIST_MIN_NAME_LENGTH) {
      return Promise.resolve(null);
    }

    const existing = this.artists().find(
      (artist) => artist.name.toLowerCase() === cleanedName.toLowerCase(),
    );

    if (existing) {
      return Promise.resolve(existing.id);
    }

    return new Promise<number | null>((resolve) => {
      this.pendingArtistResolve = resolve;
      this.pendingArtistConfirmName.set(cleanedName);
    });
  }

  protected async onArtistCreateConfirmed(): Promise<void> {
    const name = this.pendingArtistConfirmName();
    this.pendingArtistConfirmName.set(null);

    if (!name) {
      return;
    }

    try {
      const artist = await firstValueFrom(this.artistService.create({ name }));
      this.pendingArtistResolve?.(artist.id);
    } catch (error) {
      if (!(error instanceof HttpErrorResponse)) {
        throw error;
      }

      this.errorModalService.showFromHttpError(error, 'Künstler');
      this.pendingArtistResolve?.(null);
    } finally {
      this.pendingArtistResolve = null;
    }
  }

  protected onArtistCreateCancelled(): void {
    this.pendingArtistConfirmName.set(null);
    this.pendingArtistResolve?.(null);
    this.pendingArtistResolve = null;
  }

  /**
   * Löst einen Genre-Namen zu einer Id auf, analog zu {@link resolveArtistId} — inkl.
   * Bereinigung über {@link sanitizeDiscogsGenreName} (Discogs-Genres/-Styles können Kommas
   * oder andere vom Genre-Formular nicht erlaubte Zeichen enthalten).
   */
  private resolveGenreId(name: string): Promise<number | null> {
    const cleanedName = sanitizeDiscogsGenreName(name);

    if (cleanedName.length < GENRE_MIN_NAME_LENGTH) {
      return Promise.resolve(null);
    }

    const existing = this.genres().find(
      (genre) => genre.name.toLowerCase() === cleanedName.toLowerCase(),
    );

    if (existing) {
      return Promise.resolve(existing.id);
    }

    return new Promise<number | null>((resolve) => {
      this.pendingGenreResolve = resolve;
      this.pendingGenreConfirmName.set(cleanedName);
    });
  }

  protected async onGenreCreateConfirmed(): Promise<void> {
    const name = this.pendingGenreConfirmName();
    this.pendingGenreConfirmName.set(null);

    if (!name) {
      return;
    }

    try {
      const genre = await firstValueFrom(this.genreService.create({ name }));
      this.pendingGenreResolve?.(genre.id);
    } catch (error) {
      if (!(error instanceof HttpErrorResponse)) {
        throw error;
      }

      this.errorModalService.showFromHttpError(error, 'Genre');
      this.pendingGenreResolve?.(null);
    } finally {
      this.pendingGenreResolve = null;
    }
  }

  protected onGenreCreateCancelled(): void {
    this.pendingGenreConfirmName.set(null);
    this.pendingGenreResolve?.(null);
    this.pendingGenreResolve = null;
  }

  /**
   * Löst einen Label-Namen zu einer Id auf. Anders als Artist/Genre braucht ein neues Label
   * zwingend ein Herkunftsland (das Discogs nicht liefert) — die Neuanlage läuft daher über
   * das volle, vorbefüllte `LabelForm`-Modal statt über einen einfachen Bestätigungsdialog.
   * Name wird über {@link sanitizeDiscogsLabelName} bereinigt (Discogs-Labels tragen häufig
   * einen Disambiguierungs-Suffix wie „ (2)").
   */
  private resolveLabelId(name: string): Promise<number | null> {
    const cleanedName = sanitizeDiscogsLabelName(name);

    if (!cleanedName) {
      return Promise.resolve(null);
    }

    const existing = this.labels().find(
      (label) => label.name.toLowerCase() === cleanedName.toLowerCase(),
    );

    if (existing) {
      return Promise.resolve(existing.id);
    }

    return new Promise<number | null>((resolve) => {
      this.pendingLabelResolve = resolve;
      this.discogsLabelPrefillName.set(cleanedName);
      this.labelCreateOpen.set(true);
    });
  }

  protected openLabelCreate(): void {
    this.discogsLabelPrefillName.set('');
    this.labelCreateOpen.set(true);
  }

  protected onLabelCreateCancelled(): void {
    this.labelCreateOpen.set(false);
    this.discogsLabelPrefillName.set('');
    this.pendingLabelResolve?.(null);
    this.pendingLabelResolve = null;
  }

  protected onLabelCreateSaved(label: Label): void {
    this.formModel.update((model) => ({ ...model, labelId: String(label.id) }));
    this.labelAutocomplete()?.setQuery(label.name);
    this.labelCreateOpen.set(false);
    this.discogsLabelPrefillName.set('');
    this.pendingLabelResolve?.(label.id);
    this.pendingLabelResolve = null;
  }

  protected onCoverFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';

    if (!file) {
      return;
    }

    if (
      !ALLOWED_ALBUM_COVER_CONTENT_TYPES.includes(file.type) ||
      file.size > MAX_ALBUM_COVER_SIZE_BYTES
    ) {
      this.errorModalService.showValidationMessage(
        'Es sind nur JPEG- oder PNG-Dateien bis 5 MB erlaubt.',
      );
      return;
    }

    this.revokePreviewObjectUrl();
    this.selectedCoverFile.set(file);
    this.previewUrl.set(URL.createObjectURL(file));
  }

  ngOnDestroy(): void {
    this.revokePreviewObjectUrl();
  }

  private revokePreviewObjectUrl(): void {
    if (this.selectedCoverFile()) {
      URL.revokeObjectURL(this.previewUrl()!);
    }
  }

  protected onCancel(): void {
    this.cancelled.emit();
  }

  protected onDiscogsSearchOpen(): void {
    this.discogsSearchOpen.set(true);
  }

  protected onDiscogsSearchCancelled(): void {
    this.discogsSearchOpen.set(false);
  }

  protected async onDiscogsReleaseApplied(release: DiscogsRelease): Promise<void> {
    this.discogsSearchOpen.set(false);

    this.formModel.update((model) => ({
      ...model,
      albumName: release.title,
      releaseYear: release.year ? String(release.year) : model.releaseYear,
    }));

    const labelName = release.labels[0];
    const cleanedLabelName = labelName ? sanitizeDiscogsLabelName(labelName) : '';

    if (cleanedLabelName) {
      const labelId = await this.resolveLabelId(cleanedLabelName);

      if (labelId) {
        this.formModel.update((model) => ({ ...model, labelId: String(labelId) }));
        this.labelAutocomplete()?.setQuery(cleanedLabelName);
      }
    }

    const artistName = release.artists[0];
    const cleanedArtistName = artistName ? sanitizeDiscogsArtistName(artistName) : '';

    if (cleanedArtistName) {
      const recordArtistId = await this.resolveArtistId(cleanedArtistName);

      if (recordArtistId) {
        this.formModel.update((model) => ({ ...model, artistId: String(recordArtistId) }));
        this.artistDisplayName.set(cleanedArtistName);
        this.artistAutocomplete()?.setQuery(cleanedArtistName);
      }

      this.discogsRecordArtistName.set(cleanedArtistName);
    } else {
      this.discogsRecordArtistName.set(null);
    }

    const genreName = release.genres[0] ?? release.styles[0];

    this.discogsResolvedGenreId.set(genreName ? await this.resolveGenreId(genreName) : null);

    this.applyDiscogsCover(release.coverImageUrl);

    this.discogsTracklist.set(release.tracklist);
  }

  private applyDiscogsCover(coverImageUrl: string | null): void {
    if (!coverImageUrl) {
      return;
    }

    try {
      const file = dataUrlToFile(coverImageUrl, 'discogs-cover');

      this.revokePreviewObjectUrl();
      this.selectedCoverFile.set(file);
      this.previewUrl.set(URL.createObjectURL(file));
    } catch (error) {
      if (isDevMode()) {
        console.error('Discogs-Cover konnte nicht automatisch übernommen werden.', error);
      }
    }
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
      const savedRecord = record
        ? await firstValueFrom(this.recordService.update(record.id, request))
        : await firstValueFrom(this.recordService.create(request));

      await this.uploadSelectedCoverIfAny(savedRecord.id);
      await this.importDiscogsTracksIfAny(savedRecord.id);

      this.saved.emit();

      return;
    } catch (error) {
      return this.handleSaveError(error, field);
    }
  }

  private async uploadSelectedCoverIfAny(recordId: number): Promise<void> {
    const file = this.selectedCoverFile();

    if (!file) {
      return;
    }

    try {
      await firstValueFrom(this.recordService.uploadCover(recordId, file));
    } catch (error) {
      if (!(error instanceof HttpErrorResponse)) {
        throw error;
      }

      this.errorModalService.showFromHttpError(error, 'Album-Cover');
    }
  }

  /**
   * Legt für jeden gestagten Discogs-Track einen RecordTrack an. Track-Artist folgt der
   * Discogs-Realität (`track.artist`, sofern vorhanden), sonst fällt jeder Track auf den
   * bereits aufgelösten Record-Artist zurück (siehe ADR 0019). Fehlt die Genre-Zusage
   * komplett, wird der gesamte Import übersprungen (genreId ist Pflicht); fehlt nur für einen
   * bestimmten Track-Artist-Namen die Zusage, wird nur dieser Track übersprungen.
   */
  private async importDiscogsTracksIfAny(recordId: number): Promise<void> {
    const tracklist = this.discogsTracklist();

    if (tracklist.length === 0) {
      return;
    }

    const genreId = this.discogsResolvedGenreId();

    if (!genreId) {
      return;
    }

    const recordArtistName = this.discogsRecordArtistName();
    const artistIdByName = new Map<string, number>();
    const recordArtistId = Number(this.formModel().artistId) || null;

    if (recordArtistName && recordArtistId) {
      artistIdByName.set(recordArtistName.toLowerCase(), recordArtistId);
    }

    for (const [index, track] of tracklist.entries()) {
      const cleanedTrackArtistName = track.artist ? sanitizeDiscogsArtistName(track.artist) : '';
      const artistName = cleanedTrackArtistName || recordArtistName;

      if (!artistName) {
        continue;
      }

      const key = artistName.toLowerCase();
      let artistId = artistIdByName.get(key);

      if (artistId === undefined) {
        const resolved = await this.resolveArtistId(artistName);

        if (!resolved) {
          continue;
        }

        artistId = resolved;
        artistIdByName.set(key, artistId);
      }

      const { recordSide, trackNumber } = parseDiscogsPosition(track.position, index);

      try {
        await firstValueFrom(
          this.recordService.createTrack(recordId, {
            artistId,
            genreId,
            trackName: track.title,
            recordSide,
            trackNumber,
            information: track.duration ? `Dauer: ${track.duration}` : null,
          }),
        );
      } catch (error) {
        if (!(error instanceof HttpErrorResponse)) {
          throw error;
        }

        this.errorModalService.showFromHttpError(error, 'Track');
      }
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
