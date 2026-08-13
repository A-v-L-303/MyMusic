export interface ProblemDetails {
  title: string;
  detail?: string;
  status: number;
}

export interface ValidationProblemDetails {
  errors: Record<string, string[]>;
  title: string;
  status: number;
}
