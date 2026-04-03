import { PaginationState, SortingState } from "@tanstack/react-table";

import { IFetchResult } from "@/components/table/TableGeneric";

import { IFetch, PaginatedItems } from "@/lib/networking";

export type CollectionDto = {
    leadCollectionId: string;
    collectionName: string;
    createdAt?: Date;
};

export const fetchCollectionsData = async (
    options: PaginationState,
    sorting: SortingState,
    filtering: string,
    authFetch: IFetch
): Promise<IFetchResult<CollectionDto>> => {
    const url = new URL("/api/LeadCollection", window.location.origin);

    url.searchParams.append("Page", (options.pageIndex + 1).toString());
    url.searchParams.append("PageSize", options.pageSize.toString());
    url.searchParams.append("OrderBy", sorting.map((s) => `${s.id} ${s.desc ? "desc" : "asc"}`).join(","));
    url.searchParams.append("Filter", filtering);

    const response = await authFetch(url.toString(), {
        headers: {
            Accept: "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    const data: PaginatedItems<CollectionDto> = await response.json();

    return {
        rows: [...data.items],
        pageCount: data.totalPages,
        rowCount: data.totalCount
    };
};

export const createCollection = async (collection: { name: string; companyId: string | null }, authFetch: IFetch) => {
    const url = new URL("/api/LeadCollection", window.location.origin);

    const payload = {
        collectionName: collection.name,
        companyId: collection.companyId
    };

    return await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": "en-US,en;q=0.5"
        },
        body: JSON.stringify(payload),
        credentials: "include"
    });
};
