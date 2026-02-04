import { PaginationState, SortingState } from "@tanstack/react-table";

import { IFetchResult } from "@/components/table/TableGeneric";

import { IFetch, PaginatedItems } from "@/lib/networking";
import { SenderType } from "@/lib/enums";

export type CompanyDto = {
    companyId: string;
    name: string;
    smsUsername?: string;
    smsPassword?: string;
    viberPricePerMesssage: number;
    smsPricePerMesssage: number;
    apiPassword?: string;
    createdAt: Date;
};

export type UserDto = {
    id: string;
    email: string;
};

export type UserUpdatePasswordDto = {
    id: string;
    newPassword: string;
};

export type CreateUserDto = {
    email: string;
    companyId: string;
    role: number;
};

export type DeleteUserDto = {
    id: string;
};

export type SenderDto = {
    id: string;
    name: string;
    type: SenderType;
};

export type CreateSenderDto = {
    companyId: string;
    name: string;
    type: SenderType;
};

export type CompanyAnalyticsDto = {
    viberMessagesSent: number;
    smsMessagesSent: number;
    campaignCount: number;
    leadCount: number;
    totalViberCost: number;
    totalSmsCost: number;
    viberPrice: number;
    smsPrice: number;
    analyticsResults: {
        smsTotalSent: number;
        smsDelivered: number;
        smsFailed: number;
        viberTotalSent: number;
        viberDelivered: number;
        viberUndelivered: number;
        viberExpired: number;
        viberSeen: number;
        viberClicked: number;
        uniquePhoneNumbers: number;
        totalUnsubscribed: number;
        numberOfCampaigns: number;
        numberOfTests: number;
        numberOfTestsSms: number;
        numberOfTestsViber: number;
        numberOfTestsViberClicked: number;
        numberOfApiViber: number;
        numberOfApiSms: number;
    };
};

export const fetchUsersData = async (
    companyId: string,
    options: PaginationState,
    sorting: SortingState,
    authFetch: IFetch
): Promise<IFetchResult<UserDto>> => {
    const url = new URL(`/api/User`, window.location.origin);

    url.searchParams.append("Page", (options.pageIndex + 1).toString());
    url.searchParams.append("PageSize", options.pageSize.toString());
    url.searchParams.append("OrderBy", sorting.map((s) => `${s.id} ${s.desc ? "desc" : "asc"}`).join(","));
    url.searchParams.append("Filter", `CompanyIds eq ${companyId}`);

    const response = await authFetch(url.toString(), {
        headers: {
            Accept: "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    const data: PaginatedItems<UserDto> = await response.json();

    return {
        // FIX: data.items često bude readonly T[]
        rows: Array.from(data.items),
        pageCount: data.totalPages,
        rowCount: data.totalCount
    };
};

export const searchUsers = async (companyId: string, email: string, authFetch: IFetch): Promise<UserDto[]> => {
    const url = new URL(`/api/User/Search`, window.location.origin);

    url.searchParams.append("CompanyId", companyId);
    url.searchParams.append("Email", email);

    const response = await authFetch(url.toString(), {
        headers: {
            Accept: "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    const data: UserDto[] = await response.json();
    return data;
};

export const getCompanyById = async (companyId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Company/${companyId}`, window.location.origin);

    const response = await authFetch(url.toString(), {
        headers: {
            Accept: "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    const data: CompanyDto = await response.json();
    return data;
};

export const getCompanyAnalytics = async (companyId: string, authFetch: IFetch, startDate?: string, endDate?: string) => {
    const url = new URL(`/api/Company/${companyId}/Analytics`, window.location.origin);

    if (startDate) {
        url.searchParams.append("StartDate", startDate);
    }

    if (endDate) {
        url.searchParams.append("EndDate", endDate);
    }

    const response = await authFetch(url.toString(), {
        headers: {
            Accept: "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    const data: CompanyAnalyticsDto = await response.json();
    return data;
};

export const updateCompany = async (updatedData: Partial<CompanyDto>, authFetch: IFetch) => {
    const url = new URL("/api/Company", window.location.origin);

    const payload = {
        ...updatedData
    };

    return await authFetch(url.toString(), {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": "en-US,en;q=0.5"
        },
        body: JSON.stringify(payload),
        credentials: "include"
    });
};

export const deleteCompany = async (companyId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Company/${companyId}`, window.location.origin);

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

export const updateUserPassword = async (updatedData: Partial<UserUpdatePasswordDto>, authFetch: IFetch) => {
    const url = new URL("/api/User/ForcePasswordUpdate", window.location.origin);

    const payload = {
        ...updatedData
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

export const createUser = async (updatedData: Partial<CreateUserDto>, authFetch: IFetch) => {
    const url = new URL("/api/User", window.location.origin);

    const payload = {
        ...updatedData
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

export const assignUser = async (userId: string, companyId: string, authFetch: IFetch) => {
    const url = new URL("/api/User/Assign", window.location.origin);

    const payload = {
        userId,
        companyId
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

export const deleteUser = async (updatedData: Partial<DeleteUserDto>, authFetch: IFetch) => {
    const url = new URL(`/api/User/${updatedData.id}`, window.location.origin);

    return await authFetch(url.toString(), {
        method: "DELETE",
        headers: {
            "Accept-Language": "en-US,en;q=0.5"
        },
        credentials: "include"
    });
};

export const getCompanySenders = async (companyId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Company/${companyId}/Senders`, window.location.origin);

    const response = await authFetch(url.toString(), {
        headers: {
            Accept: "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    const data: SenderDto[] = await response.json();
    return data;
};

export const createSender = async (data: Partial<CreateSenderDto>, authFetch: IFetch) => {
    const url = new URL("/api/Sender", window.location.origin);

    const payload = {
        ...data
    };

    const response = await authFetch(url.toString(), {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept-Language": "en-US,en;q=0.5"
        },
        body: JSON.stringify(payload),
        credentials: "include"
    });

    return await response.json();
};

export const deleteSender = async (senderId: string, authFetch: IFetch) => {
    const url = new URL(`/api/Sender/${senderId}`, window.location.origin);

    return await authFetch(url.toString(), {
        method: "DELETE",
        headers: {
            "Accept-Language": "en-US,en;q=0.5"
        },
        credentials: "include"
    });
};
