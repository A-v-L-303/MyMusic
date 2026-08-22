export interface DiscogsSearchResult {
  id: number;
  title: string;
  year: number | null;
  label: string | null;
  thumbnailUrl: string | null;
}

export interface DiscogsFormat {
  name: string;
  descriptions: string[];
}

export interface DiscogsTrack {
  position: string;
  title: string;
  duration: string | null;
  artist: string | null;
}

export interface DiscogsRelease {
  id: number;
  title: string;
  year: number | null;
  artists: string[];
  labels: string[];
  genres: string[];
  styles: string[];
  formats: DiscogsFormat[];
  coverImageUrl: string | null;
  tracklist: DiscogsTrack[];
}
