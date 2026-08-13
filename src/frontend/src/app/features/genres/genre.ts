export interface Genre {
  id: number;
  name: string;
}

export interface GenreListResponse {
  items: Genre[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateGenreRequest {
  name: string;
}

export interface UpdateGenreRequest {
  name: string;
}
