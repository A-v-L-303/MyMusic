import { RecordFormat } from '../records/record';

export interface FormatCount {
  format: RecordFormat;
  count: number;
}

export interface TopArtist {
  artistId: number;
  artistName: string;
  count: number;
}

export interface TopLabel {
  labelId: number;
  labelName: string;
  count: number;
}

export interface YearCount {
  year: number;
  count: number;
}

export interface DashboardStats {
  recordsTotal: number;
  artistsTotal: number;
  labelsTotal: number;
  genresTotal: number;
  formatDistribution: FormatCount[];
  topArtists: TopArtist[];
  topLabels: TopLabel[];
  yearDistribution: YearCount[];
}
