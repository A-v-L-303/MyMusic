export interface Artist {
  id: number;
  name: string;
}

export interface ArtistListResponse {
  items: Artist[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateArtistRequest {
  name: string;
}

export interface UpdateArtistRequest {
  name: string;
}
