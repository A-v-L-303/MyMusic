export interface AdminUser {
  id: string;
  username: string;
  email: string;
  role: 'User' | 'Admin';
}

export interface AdminUserListResponse {
  items: AdminUser[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
