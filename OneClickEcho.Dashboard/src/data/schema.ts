export type Option = {
    value: string;
    label: string;
};

export type KpiKey =
    | "systemReceived"
    | "pending"
    | "sent"
    | "delivered"
    | "failed"
    | "undelivered"
    | "expired"
    | "seen"
    | "clicked"
    | "unsubscribed"
    | "blacklisted"
    | "error"
    | "other";

export type KpiEntry = {
    key: KpiKey; // ✅ stabilan ključ (za logiku + prevod)
    title: string; // za prikaz (obično tKpi(key))
    percentage: number;
    current: number;
    allowed: number;
    unit?: string;
};

export type KpiEntryExtended = Omit<KpiEntry, "current" | "allowed" | "unit"> & {
    value: string;
    color: string;
};
