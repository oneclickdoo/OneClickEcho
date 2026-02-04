import {PaginationState, SortingState} from "@tanstack/react-table";

import {IFetchResult} from "@/components/table/TableGeneric";

import {IFetch, PaginatedItems} from "@/lib/networking";
import {ApiMessageType} from "@/lib/enums";

export type ApiMessageDto = {
    apiMessageId: string;
    companyId: string;
    phoneNumber: string;
    message: string;
    messageType: ApiMessageType;
    hasSmsFallback: boolean;
    viberSender?: string;
    viberMedia?: string;
    viberButtonUrl?: string;
    viberButtonUrlTitle?: string;
    viberMessageId: number;
    viberStatus: number;
    viberStatusDescription?: string;
    smsStatus: number;
    smsStatusDescription?: string;
    smsReferenceId?: string;
    createdAt?: Date;
};

export const fetchApiMessagesData = async (
    options: PaginationState,
    sorting: SortingState,
    filtering: string,
    authFetch: IFetch
): Promise<IFetchResult<ApiMessageDto>> => {
    const url = new URL("/api/Message", window.location.origin);

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

    const data: PaginatedItems<ApiMessageDto> = await response.json();

    return {
        rows: data.items,
        pageCount: data.totalPages,
        rowCount: data.totalCount
    };
};