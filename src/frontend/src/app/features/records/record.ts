export type RecordFormat =
  | 'Album'
  | 'MaxiSingle'
  | 'Single'
  | 'Ep'
  | 'Compilation'
  | 'CdAlbum'
  | 'CdMaxiSingle'
  | 'CdSingle'
  | 'CdEp'
  | 'CdCompilation';

export type RecordCondition = 'Mint' | 'Nm' | 'VgPlus' | 'Vg' | 'GPlus' | 'G' | 'P';

export interface RecordTrack {
  id: number;
  recordId: number;
  artistId: number;
  artistName: string;
  genreId: number;
  genreName: string;
  trackName: string;
  recordSide: string;
  trackNumber: number;
  information: string | null;
}

export interface Record {
  id: number;
  labelId: number;
  labelName: string;
  artistId: number | null;
  artistName: string | null;
  format: RecordFormat;
  albumName: string;
  releaseYear: number;
  condition: RecordCondition;
  information: string | null;
  albumCoverDataUrl: string | null;
  tracks: RecordTrack[];
}

export interface RecordListResponse {
  items: Record[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const RECORD_FORMAT_LABELS: { [format in RecordFormat]: string } = {
  Album: 'Album',
  MaxiSingle: 'MaxiSingle',
  Single: 'Single',
  Ep: 'EP',
  Compilation: 'Compilation',
  CdAlbum: 'CD-Album',
  CdMaxiSingle: 'CD-MaxiSingle',
  CdSingle: 'CD-Single',
  CdEp: 'CD-EP',
  CdCompilation: 'CD-Compilation',
};

export const VINYL_FORMATS: RecordFormat[] = ['Album', 'MaxiSingle', 'Single', 'Ep', 'Compilation'];

export const CD_FORMATS: RecordFormat[] = [
  'CdAlbum',
  'CdMaxiSingle',
  'CdSingle',
  'CdEp',
  'CdCompilation',
];

export const RECORD_CONDITION_LABELS: { [condition in RecordCondition]: string } = {
  Mint: 'Mint (M)',
  Nm: 'Near Mint (NM)',
  VgPlus: 'Very Good Plus',
  Vg: 'Very Good',
  GPlus: 'Good Plus',
  G: 'Good',
  P: 'Poor',
};

// CSS-Klassen aus src/styles/design-system/components.css — .grade-gplus existiert dort nicht,
// die tatsächliche Klasse für "Good Plus" heißt .grade-f (Abweichung von der Wiki-Doku gemeldet).
export const RECORD_CONDITION_GRADE_CLASS: { [condition in RecordCondition]: string } = {
  Mint: 'grade-m',
  Nm: 'grade-nm',
  VgPlus: 'grade-vgp',
  Vg: 'grade-vg',
  GPlus: 'grade-f',
  G: 'grade-g',
  P: 'grade-p',
};

export const RECORD_CONDITION_GRADE_TEXT: { [condition in RecordCondition]: string } = {
  Mint: 'M',
  Nm: 'NM',
  VgPlus: 'VG+',
  Vg: 'VG',
  GPlus: 'G+',
  G: 'G',
  P: 'P',
};
