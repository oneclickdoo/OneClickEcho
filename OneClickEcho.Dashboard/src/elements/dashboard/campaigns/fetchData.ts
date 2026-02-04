import { PaginationState, SortingState } from "@tanstack/react-table";
import moment from "moment";

import { IFetchResult } from "@/components/table/TableGeneric";
import { convertDateOnlyToDate } from "@/lib/utils";

import { IFetch, PaginatedItems } from "@/lib/networking";
import { CampaignSendingType } from "@/lib/enums";

export type CampaignsDto = {
    campaignId: string;
    companyId: string;

    // Backend can return numeric enum OR i18n key (string)
    status: unknown;

    name: string;
    sendingDatetime?: string;
    sendingDatetimeObject?: Date;
    createdAt?: Date;
};

export const fetchCampaignsData = async (
    options: PaginationState,
    sorting: SortingState,
    filtering: string,
    authFetch: IFetch,
    t: (key: string) => string
): Promise<IFetchResult<CampaignsDto>> => {
    const url = new URL("/api/Campaign", window.location.origin);

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
        throw new Error(t("Errors.networkResponseNotOk"));
    }

    const data: PaginatedItems<CampaignsDto> = await response.json();

    // PaginatedItems.items can be readonly -> create a mutable copy for IFetchResult.rows
    const items: CampaignsDto[] = data.items.map((item) => ({
        ...item,
        sendingDatetimeObject: item.sendingDatetime ? convertDateOnlyToDate(item.sendingDatetime) : undefined
    }));

    return {
        rows: items,
        pageCount: data.totalPages,
        rowCount: data.totalCount
    };
};

export const createCampaign = async (campaign: { name: string; companyId: string | null }, authFetch: IFetch) => {
    const url = new URL("/api/Campaign", window.location.origin);

    const payload = {
        CampaignName: campaign.name,
        sendingType: CampaignSendingType.Immediate,
        sendingDatetime: moment().add(7, "days").toISOString(),
        companyId: campaign.companyId
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
