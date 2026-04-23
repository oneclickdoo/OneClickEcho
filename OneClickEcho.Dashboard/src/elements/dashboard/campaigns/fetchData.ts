import moment from "moment";
import { PaginationState, SortingState } from "@tanstack/react-table";
import { IFetchResult } from "@/components/table/TableGeneric";
import { convertDateOnlyToDate } from "@/lib/utils";
import { IFetch, PaginatedItems } from "@/lib/networking";
import { CampaignSendingType } from "@/lib/enums";

/** First calendar year shown in the campaigns year filter (inclusive). */
export const CAMPAIGNS_FILTER_MIN_YEAR = 2025;

/** OData fragment: campaigns created within the given calendar year (matches FilterManager date style). */
export function buildCreatedYearFilter(year: number): string {
    const from = new Date(year, 0, 1, 0, 0, 0, 0);
    const to = new Date(year, 11, 31, 23, 59, 59, 999);
    const upperExclusive = moment(to).add(1, "days").toISOString();
    return `CreatedAt ge ${from.toISOString()} and CreatedAt le ${upperExclusive}`;
}

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
    companyId: string,
    authFetch: IFetch,
    t: (key: string) => string,
    createdInYear?: number
): Promise<IFetchResult<CampaignsDto>> => {
    if (!companyId) {
        throw new Error("CompanyId is required.");
    }

    const url = new URL("/api/Campaign", window.location.origin);

    url.searchParams.append("Page", (options.pageIndex + 1).toString());
    url.searchParams.append("PageSize", options.pageSize.toString());
    url.searchParams.append("CompanyId", companyId);

    if (sorting.length > 0) {
        url.searchParams.append(
            "OrderBy",
            sorting.map((s) => `${s.id} ${s.desc ? "desc" : "asc"}`).join(",")
        );
    }

    const filterParts: string[] = [];
    if (filtering && filtering.trim().length > 0) {
        filterParts.push(filtering.trim());
    }
    if (createdInYear != null) {
        const maxY = new Date().getFullYear();
        const y = Math.min(Math.max(createdInYear, CAMPAIGNS_FILTER_MIN_YEAR), maxY);
        filterParts.push(buildCreatedYearFilter(y));
    }
    if (filterParts.length > 0) {
        url.searchParams.append("Filter", filterParts.join(" and "));
    }

    const response = await authFetch(url.toString(), {
        headers: {
            Accept: "application/json"
        }
    });

    if (!response.ok) {
        throw new Error(t("errors.networkResponseNotOk"));
    }

    const data: PaginatedItems<CampaignsDto> = await response.json();

    const items: CampaignsDto[] = data.items.map((item) => ({
        ...item,
        sendingDatetimeObject: item.sendingDatetime
            ? convertDateOnlyToDate(item.sendingDatetime)
            : undefined
    }));

    return {
        rows: items,
        pageCount: data.totalPages,
        rowCount: data.totalCount
    };
};

export const createCampaign = async (
    campaign: { name: string; companyId: string },
    authFetch: IFetch
) => {
    if (!campaign.companyId) {
        throw new Error("CompanyId is required.");
    }

    const url = new URL("/api/Campaign", window.location.origin);

    const payload = {
        campaignName: campaign.name,
        companyId: campaign.companyId,
        isViber: true,
        fallbackToSMS: false,
        isViberReceivable: false,
        viberSender: "Default",
        viberMessage: "Draft message",
        viberButtonUrl: null,
        viberButtonUrlTitle: null,
        viberVideoThumbnail: null,
        isSms: false,
        smsSender: null,
        smsMessage: null,
        testPhoneNumbers: null,
        sendingType: CampaignSendingType.Immediate,
        sendingDatetime: null
    };

    console.log("createCampaign payload", payload);
    console.log("createCampaign json", JSON.stringify(payload));

    return await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Accept: "application/json",
            "Accept-Language": "en-US,en;q=0.5"
        },
        body: JSON.stringify(payload),
        credentials: "include"
    });
};