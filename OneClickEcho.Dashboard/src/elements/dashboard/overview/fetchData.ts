import { IFetch } from "@/lib/networking";

export type AdminAnalyticsDto = {
    totalViberMessagesSent: number;
    totalViberMessagesDelivered: number;
    totalSmsMessagesSent: number;
    totalSmsMessagesDelivered: number;
    totalRevenue: number;
    totalProfit: number;
    companies: AdminCompanyAnalyticsDto[];
};

export type AdminCompanyAnalyticsDto = {
    companyId: string;
    name: string;
    viberMessagesSent: number;
    viberMessagesDelivered: number;
    smsMessagesSent: number;
    smsMessagesDelivered: number;
    revenue: number;
    profit: number;
};

export const getAdminAnalytics = async (authFetch: IFetch, startDate?: string, endDate?: string) => {
    const url = new URL("/api/Admin/Analytics", window.location.origin);

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

    let data: AdminAnalyticsDto = await response.json();

    return data;
};
