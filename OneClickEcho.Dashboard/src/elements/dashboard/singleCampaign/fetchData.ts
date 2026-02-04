import type { PaginationState, SortingState } from "@tanstack/react-table";

import type { IFetchResult } from "@/components/table/TableGeneric";

import { convertDateOnlyToDate } from "@/lib/utils";

import type { IFetch, PaginatedItems } from "@/lib/networking";
import {
    CampaignLeadSMSStatus,
    CampaignLeadViberStatusCollection,
    CampaignSendingType,
    CampaignStatus,
    LeadGender
} from "@/lib/enums";

/**
 * Helper: Accept-Language (nemamo next-intl hook ovde, pa uzimamo iz browser-a).
 * Backend će tako vraćati lokalizovane poruke ako podržava.
 */
const getAcceptLanguage = () => {
    if (typeof window === "undefined") return "en-US,en;q=0.5";
    return navigator.language ? `${navigator.language},en;q=0.5` : "en-US,en;q=0.5";
};

/** Helper: YYYY-MM-DD (date-only) */
const formatDateOnly = (d: Date) => d.toISOString().slice(0, 10);

export type CampaignDto = {
    campaignId: string;
    companyId: string;
    status: CampaignStatus;
    name: string;

    isViber: boolean;
    fallbackToSMS: boolean;
    isViberReceivable: boolean;

    viberSender?: string;
    viberMessage?: string;
    isTransactional?: boolean;
    viberMedia?: string;
    viberButtonUrl?: string;
    viberButtonUrlTitle?: string;
    viberVideoThumbnail?: string;
    viberValidity?: number;

    isSms: boolean;
    smsSender?: string;
    smsMessage?: string;

    testPhoneNumber?: string;

    sendingType: CampaignSendingType;
    sendingDatetime?: string;
    sendingDatetimeObject?: Date; // Created in the frontend
};

export type CampaignLeadDto = {
    leadId: string;
    phoneNumber: string;
    firstName?: string;
    lastName?: string;
    gender?: LeadGender;
    email?: string;
    dateOfBirth?: string;
    dateOfBirthObject?: Date; // Created in the frontend
    city?: string;
    state?: string;
    country?: string;
    isBlacklisted?: boolean;
};

export type CampaignLeadCollectionDto = {
    campaignId: string;
    leadCollectionId: string;
    name: string;
    createdAt: Date;
};

export type LeadCollectionDto = {
    leadCollectionId: string;
    collectionName: string;
    createdAt?: string;
};

export type CreateLeadDto = {
    companyId: string;
    campaignId?: string;
    phoneNumber: string;
    firstName?: string;
    lastName?: string;
    gender?: LeadGender;
    email?: string;
    dateOfBirth?: string;
    city?: string;
    state?: string;
    country?: string;
};

export type CampaignAnalytics = {
    viber?: CampaignViberAnalytics;
    sms?: CampaignSmsAnalytics;
};

export type CampaignViberAnalytics = {
    pending: number;
    delivered: number;
    undelivered: number;
    seen: number;
    received: number;
    clicked: number;
    expired: number;
    unsubscribed: number;
    total: number;
};

export type CampaignSmsAnalytics = {
    pending: number;
    delivered: number;
    undelivered: number;
    blacklisted: number;
    error: number;
    total: number;
};

export type GptResponse = {
    gptRequestId: string;
    responseMessage: string;
};

export const fetchLeadsData = async (
    options: PaginationState,
    sorting: SortingState,
    filtering: string,
    campaignId: string | null,
    authFetch: IFetch
): Promise<IFetchResult<CampaignLeadDto>> => {
    const url = campaignId
        ? new URL(`/api/Campaign/${campaignId}/Leads`, window.location.origin)
        : new URL(`/api/Lead`, window.location.origin);

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

    const data: PaginatedItems<CampaignLeadDto> = await response.json();

    // ✅ FIX: ne mutiramo data.items (može biti readonly)
    const items: CampaignLeadDto[] = data.items.map((item) => ({
        ...item,
        dateOfBirthObject: item.dateOfBirth ? convertDateOnlyToDate(item.dateOfBirth) : undefined
    }));

    return {
        rows: items,
        pageCount: data.totalPages,
        rowCount: data.totalCount
    };
};

export const fetchCampaignLeadCollectionsData = async (
    campaignId: string,
    options: PaginationState,
    sorting: SortingState,
    filtering: string,
    authFetch: IFetch
) => {
    const url = new URL(`/api/Campaign/${campaignId}/LeadCollections`, window.location.origin);

    url.searchParams.append("Page", (options.pageIndex + 1).toString());
    url.searchParams.append("PageSize", options.pageSize.toString());
    url.searchParams.append("OrderBy", sorting.map((s) => `${s.id} ${s.desc ? "desc" : "asc"}`).join(","));
    url.searchParams.append("Filter", filtering);

    const response = await authFetch(url.toString(), {
        headers: { Accept: "application/json" }
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    const data: PaginatedItems<LeadCollectionDto & { count?: number }> = await response.json();

    // Count endpoint
    const urlCount = new URL(`/api/LeadCollection/Count`, window.location.origin);
    const countResponse = await authFetch(urlCount.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage(),
            Accept: "application/json"
        },
        body: JSON.stringify({
            leadCollectionIds: data.items.map((item) => item.leadCollectionId)
        })
    });

    if (!countResponse.ok) {
        throw new Error("Network response was not ok");
    }

    const dataCount: { items: Array<{ leadCollectionId: string; count: number }> } = await countResponse.json();

    const countById = new Map<string, number>();
    dataCount.items.forEach((it) => countById.set(it.leadCollectionId, it.count));

    // ✅ FIX: ne mutiramo data.items (može biti readonly)
    const items: Array<LeadCollectionDto & { count?: number }> = data.items.map((item) => ({
        ...item,
        count: countById.get(item.leadCollectionId) ?? 0
    }));

    return {
        rows: items,
        pageCount: data.totalPages,
        rowCount: data.totalCount
    };
};

export const searchLeadCollections = async (
    campaignId: string,
    name: string,
    authFetch: IFetch
): Promise<LeadCollectionDto[]> => {
    const url = new URL(`/api/LeadCollection/Search`, window.location.origin);

    url.searchParams.append("CampaignId", campaignId);
    url.searchParams.append("Name", name);

    const response = await authFetch(url.toString(), {
        headers: { Accept: "application/json" }
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    return await response.json();
};

export const assignCampaignLeadCollection = async (campaignId: string, leadCollectionId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}/LeadCollections`, window.location.origin);

    const response = await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage(),
            Accept: "application/json"
        },
        body: JSON.stringify({ leadCollectionId })
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    return response;
};

export const deleteCampaignLeadCollection = async (campaignId: string, leadCollectionId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}/LeadCollections/${leadCollectionId}`, window.location.origin);

    const response = await authFetch(url, {
        method: "DELETE",
        headers: { Accept: "application/json" },
        credentials: "include"
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    return response;
};

export const exportCompanyLeads = async (companyId: string, authFetch: IFetch): Promise<Blob> => {
    const url = new URL(`/api/Company/${companyId}/ExportLeads`, window.location.origin);

    const response = await authFetch(url.toString());
    if (!response.ok) throw new Error("Network response was not ok");

    return await response.blob();
};

export const exportCompanyLeadsByCollection = async (
    companyId: string,
    collectionId: string,
    authFetch: IFetch
): Promise<Blob> => {
    const url = new URL(`/api/Company/${companyId}/ExportLeadsByCollection/${collectionId}`, window.location.origin);

    const response = await authFetch(url.toString());
    if (!response.ok) throw new Error("Network response was not ok");

    return await response.blob();
};

export const exportCampaignLeads = async (campaignId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}/ExportLeads`, window.location.origin);

    const response = await authFetch(url.toString());
    if (!response.ok) throw new Error("Network response was not ok");

    return await response.blob();
};

export const createLead = async (lead: CreateLeadDto, authFetch: IFetch) => {
    const url = new URL("/api/Lead", window.location.origin);

    return await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage(),
            Accept: "application/json"
        },
        body: JSON.stringify(lead)
    });
};

export const updateLead = async (lead: CampaignLeadDto, authFetch: IFetch) => {
    const url = new URL(`/api/Lead`, window.location.origin);

    const payload = {
        ...lead,
        dateOfBirth: lead.dateOfBirthObject ? formatDateOnly(lead.dateOfBirthObject) : undefined
    };

    const response = await authFetch(url.toString(), {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            Accept: "application/json"
        },
        body: JSON.stringify(payload)
    });

    if (!response.ok) throw new Error("Network response was not ok");

    return await response.json();
};

export const deleteLead = async (leadId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Lead/${leadId}`, window.location.origin);

    const response = await authFetch(url, {
        method: "DELETE",
        headers: { Accept: "application/json" },
        credentials: "include"
    });

    if (!response.ok) throw new Error("Network response was not ok");

    return response;
};

export const uploadLeads = async (file: File, companyId: string | null, campaignId: string | null, authFetch: IFetch) => {
    const url = new URL("/api/Lead/UploadLeads", window.location.origin);

    const formData = new FormData();
    formData.append("file", file);
    formData.append("companyId", companyId!);

    if (campaignId) formData.append("campaignId", campaignId);

    const response = await authFetch(url.toString(), {
        method: "POST",
        body: formData,
        credentials: "include"
    });

    if (!response.ok) throw new Error("Network response was not ok");

    return await response.json();
};

export const downloadExampleLeadCsv = async (authFetch: IFetch) => {
    const url = new URL("/api/Lead/Download", window.location.origin);

    const response = await authFetch(url.toString());
    if (!response.ok) throw new Error("Network response was not ok");

    return await response.blob();
};

export const getCampaignById = async (campaignId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}`, window.location.origin);

    const response = await authFetch(url.toString(), {
        headers: { Accept: "application/json" }
    });

    if (!response.ok) throw new Error("Network response was not ok");

    let data: CampaignDto = await response.json();

    data = {
        ...data,
        sendingDatetimeObject: data.sendingDatetime ? new Date(data.sendingDatetime) : undefined
    };

    return data;
};

export const getCampaignAnalytics = async (campaignId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}/Analytics`, window.location.origin);

    const response = await authFetch(url.toString(), {
        headers: { Accept: "application/json" }
    });

    if (!response.ok) throw new Error("Network response was not ok");

    const data: CampaignAnalytics = await response.json();
    return data;
};

export const createCollectionFromStatus = async (
    companyId: string,
    campaignId: string,
    viberStatus: CampaignLeadViberStatusCollection | null,
    smsStatus: CampaignLeadSMSStatus | null,
    collectionName: string,
    authFetch: IFetch
) => {
    const url = new URL(`/api/Campaign/${campaignId}/CreateCollection`, window.location.origin);

    const payload = {
        companyId,
        viberStatus,
        smsStatus,
        collectionName
    };

    return await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage(),
            Accept: "application/json"
        },
        body: JSON.stringify(payload)
    });
};

export const addToCollectionFromStatus = async (
    campaignId: string,
    leadCollectionId: string,
    viberStatus: CampaignLeadViberStatusCollection | null,
    smsStatus: CampaignLeadSMSStatus | null,
    authFetch: IFetch
) => {
    const url = new URL(`/api/Campaign/${campaignId}/AddToCollection`, window.location.origin);

    const payload = {
        campaignId,
        leadCollectionId,
        viberStatus,
        smsStatus
    };

    return await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage(),
            Accept: "application/json"
        },
        body: JSON.stringify(payload)
    });
};

export const exportFromStatus = async (
    campaignId: string,
    viberStatus: CampaignLeadViberStatusCollection | null,
    smsStatus: CampaignLeadSMSStatus | null,
    authFetch: IFetch
) => {
    const url = new URL(`/api/Campaign/${campaignId}/ExportFromStatus`, window.location.origin);

    const payload = { viberStatus, smsStatus };

    const response = await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage(),
            Accept: "application/json"
        },
        body: JSON.stringify(payload)
    });

    if (!response.ok) throw new Error("Network response was not ok");

    return await response.blob();
};

export const updateCampaign = async (updatedData: Partial<CampaignDto>, authFetch: IFetch) => {
    const url = new URL("/api/Campaign", window.location.origin);

    const payload = {
        ...updatedData,
        sendingDatetime: updatedData.sendingDatetimeObject ? updatedData.sendingDatetimeObject.toISOString() : undefined
    };

    const response = await authFetch(url.toString(), {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage()
        },
        body: JSON.stringify(payload),
        credentials: "include"
    });

    if (!response.ok) throw new Error("Network response was not ok");

    return await response.json();
};

export const endCampaign = async (campaignId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}/End`, window.location.origin);

    const response = await authFetch(url.toString(), {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage()
        },
        credentials: "include"
    });

    const data = await response.json();
    if (!response.ok) throw new Error(data.message);

    return data;
};

export const launchCampaign = async (campaignId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}/Launch`, window.location.origin);

    const response = await authFetch(url.toString(), {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage()
        },
        credentials: "include"
    });

    const data = await response.json();
    if (!response.ok) throw new Error(data.message);

    return data;
};

export const pauseCampaign = async (campaignId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}/Pause`, window.location.origin);

    const response = await authFetch(url.toString(), {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage()
        },
        credentials: "include"
    });

    const data = await response.json();
    if (!response.ok) throw new Error(data.message);

    return data;
};

export const testCampaign = async (campaignId: string, phoneNumber: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}/Test`, window.location.origin);

    return await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage()
        },
        credentials: "include",
        body: JSON.stringify({ phoneNumber })
    });
};

export const cloneCampaign = async (campaignId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}/Clone`, window.location.origin);

    const response = await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage()
        },
        credentials: "include"
    });

    const data = await response.json();
    if (!response.ok) throw new Error(data.message);

    return data;
};

export const uploadCampaignViberMedia = async (
    campaignId: string,
    isThumbnail: boolean,
    file: File,
    authFetch: IFetch,
    duration?: number
) => {
    const url = new URL(
        `/api/Campaign/${campaignId}/Upload?isThumbnail=${isThumbnail}${duration ? `&duration=${duration}` : ""}`,
        window.location.origin
    );

    const formData = new FormData();
    formData.append("file", file);

    const response = await authFetch(url.toString(), {
        method: "PUT",
        body: formData,
        credentials: "include"
    });

    if (!response.ok) throw new Error("Network response was not ok");

    return await response.json();
};

export const deleteCampaign = async (campaignId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Campaign/${campaignId}`, window.location.origin);

    const response = await authFetch(url, {
        method: "DELETE",
        headers: { Accept: "application/json" },
        credentials: "include"
    });

    if (!response.ok) throw new Error("Network response was not ok");

    return response;
};

export const generateMessage = async (campaignId: string, message: string, authFetch: IFetch) => {
    const url = new URL("/api/GptRequest/Generate", window.location.origin);

    const payload = {
        campaignId,
        requestMessage: message
    };

    const response = await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage(),
            Accept: "application/json"
        },
        body: JSON.stringify(payload)
    });

    const data: GptResponse = await response.json();
    if (!response.ok) throw new Error("Network response was not ok");

    return data;
};

export const enhanceMessage = async (campaignId: string, message: string, authFetch: IFetch) => {
    const url = new URL("/api/GptRequest/Enhance", window.location.origin);

    const payload = {
        campaignId,
        requestMessage: message
    };

    const response = await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": getAcceptLanguage(),
            Accept: "application/json"
        },
        body: JSON.stringify(payload)
    });

    const data: GptResponse = await response.json();
    if (!response.ok) throw new Error("Network response was not ok");

    return data;
};
