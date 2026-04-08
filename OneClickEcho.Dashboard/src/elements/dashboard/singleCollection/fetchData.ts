import { PaginationState, SortingState } from "@tanstack/react-table";

import { IFetchResult } from "@/components/table/TableGeneric";

import { CollectionDto } from "../collections/fetchData";
import { CampaignLeadDto } from "../singleCampaign/fetchData";

import { IFetch, PaginatedItems } from "@/lib/networking";

import { LeadGender } from "@/lib/enums";

export type SingleLeadDto = {
    leadId: string;
};

export type CollectionAssignLeadsDto = {
    leadCollectionId: string;
    leads: SingleLeadDto[];
};

export type CreateAndAssignLeadDto = {
    companyId: string;
    leadCollectionId: string;
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

export const getCollectionById = async (collectionId: string, authFetch: IFetch) => {
    const url = new URL(`/api/LeadCollection/${collectionId}`, window.location.origin);

    const response = await authFetch(url.toString(), {
        headers: {
            Accept: "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    const data: CollectionDto = await response.json();
    return data;
};

export const getCollectionLeads = async (
    options: PaginationState,
    sorting: SortingState,
    filtering: string,
    collectionId: string,
    authFetch: IFetch
): Promise<IFetchResult<CampaignLeadDto>> => {
    const url = new URL(`/api/LeadCollection/${collectionId}/Leads`, window.location.origin);

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

    return {
        // FIX: API tip je često readonly T[]; IFetchResult očekuje mutable T[]
        rows: Array.from(data.items),
        pageCount: data.totalPages,
        rowCount: data.totalCount
    };
};

export const updateCollection = async (updatedData: Partial<CollectionDto>, authFetch: IFetch) => {
    const url = new URL("/api/LeadCollection", window.location.origin);

    const response = await authFetch(url.toString(), {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            Accept: "application/json",
            "Accept-Language": "en-US,en;q=0.5"
        },
        body: JSON.stringify(updatedData),
        credentials: "include"
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    return await response.json();
};

export const assignCollectionLeads = async (data: CollectionAssignLeadsDto, authFetch: IFetch) => {
    const url = new URL("/api/LeadCollection/AssignLeads", window.location.origin);

    const response = await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            Accept: "application/json",
            "Accept-Language": "en-US,en;q=0.5"
        },
        body: JSON.stringify(data),
        credentials: "include"
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    return await response.json();
};

export const uploadAndAssignCollectionLeads = async (file: File, collectionId: string, authFetch: IFetch) => {
    const url = new URL("/api/LeadCollection/UploadAndAssignLeads", window.location.origin);

    const formData = new FormData();
    formData.append("file", file);
    formData.append("LeadCollectionId", collectionId);

    const response = await authFetch(url.toString(), {
        method: "POST",
        body: formData,
        credentials: "include"
    });

    if (!response.ok) {
        let detail = "";
        try {
            detail = (await response.clone().text()).slice(0, 500);
        } catch {
            /* ignore */
        }
        throw new Error(
            detail
                ? `Collection CSV upload failed (HTTP ${response.status}): ${detail}`
                : `Collection CSV upload failed (HTTP ${response.status})`
        );
    }

    return await response.json();
};

export const deleteCollection = async (collectionId: string, authFetch: IFetch) => {
    const url = new URL(`/api/LeadCollection/${collectionId}`, window.location.origin);

    const response = await authFetch(url.toString(), {
        method: "DELETE",
        headers: {
            Accept: "application/json"
        },
        credentials: "include"
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    return response;
};

export const createAndAssignLead = async (lead: CreateAndAssignLeadDto, authFetch: IFetch) => {
    const url = new URL("/api/LeadCollection/CreateAndAssignLead", window.location.origin);

    return await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": "en-US,en;q=0.5",
            Accept: "application/json"
        },
        body: JSON.stringify(lead)
    });
};
