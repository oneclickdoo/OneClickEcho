import { IFetchResult, IFetchTable } from "@/components/table/TableGeneric";

import { IFetch, PaginatedItems } from "@/lib/networking";

export type CompanyDto = {
    companyId: string;
    smsUsername?: string;
    smsPassword?: string;
    viberPricePerMesssage: number;
    smsPricePerMesssage: number;
    name: string;
    createdAt: Date;
};

export const fetchCompaniesData: IFetchTable<CompanyDto> = async (options, sorting, authFetch): Promise<IFetchResult<CompanyDto>> => {
    const url = new URL(`/api/Company`, window.location.origin);

    url.searchParams.append("Page", (options.pageIndex + 1).toString());
    url.searchParams.append("PageSize", options.pageSize.toString());
    url.searchParams.append("OrderBy", sorting.map((s) => `${s.id} ${s.desc ? "desc" : "asc"}`).join(","));
    // url.searchParams.append('Filter', "FirstName co 'J'");

    const response = await authFetch(url.toString(), {
        headers: {
            Accept: "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    const data: PaginatedItems<CompanyDto> = await response.json();

    return {
        rows: [...data.items],
        pageCount: data.totalPages,
        rowCount: data.totalCount
    };
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

    let data: CompanyDto = await response.json();

    return data;
};

export const createCompany = async (name: string, authFetch: IFetch) => {
    const url = new URL(`/api/Company`, window.location.origin);

    const payload = {
        name: name
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
