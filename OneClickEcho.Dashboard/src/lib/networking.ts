export interface PaginatedItems<T> {
    items: ReadonlyArray<T>;
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
}

/**
 * Fetch-like function signature (useful for DI/mocking in tests).
 * Equivalent to: typeof fetch
 */
export type IFetch = (input: RequestInfo | URL, init?: RequestInit) => Promise<Response>;
