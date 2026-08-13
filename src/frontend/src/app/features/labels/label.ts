export interface Label {
  id: number;
  name: string;
  countryId: number;
  countryName: string;
  information: string | null;
}

export interface LabelListResponse {
  items: Label[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateLabelRequest {
  name: string;
  countryId: number;
  information: string | null;
}

export interface UpdateLabelRequest {
  name: string;
  countryId: number;
  information: string | null;
}
