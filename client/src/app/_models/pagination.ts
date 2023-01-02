// these keys are the same with the keys of response pagination header
// that client is receiving from API

export interface Pagination {
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

// we made this generic because it's not just our MemberDto
// we're going to want to paginate
export class PaginatedResult<T> {
    result?: T;
    pagination?: Pagination;
}