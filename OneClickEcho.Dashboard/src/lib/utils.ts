// Tremor Raw cx [v0.0.0]

import clsx, { type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

import { CampaignMediaType } from "@/lib/enums";

export function cx(...args: ClassValue[]) {
    return twMerge(clsx(...args));
}

// Tremor Raw focusInput [v0.0.1]

export const focusInput = [
    // base
    "focus:ring-2",
    // ring color
    "focus:ring-indigo-200 focus:dark:ring-indigo-700/30",
    // border color
    "focus:border-indigo-500 focus:dark:border-indigo-700"
] as const;

// Tremor Raw focusRing [v0.0.1]

export const focusRing = [
    // base
    "outline outline-offset-2 outline-0 focus-visible:outline-2",
    // outline color
    "outline-indigo-500 dark:outline-indigo-500"
] as const;

// Tremor Raw hasErrorInput [v0.0.1]

export const hasErrorInput = [
    // base
    "ring-2",
    // border color
    "border-red-500 dark:border-red-700",
    // ring color
    "ring-red-200 dark:ring-red-700/30"
] as const;

// --------------------------------------------------------
// Number formatters
// --------------------------------------------------------

export const usNumberformatter = (value: number, decimals = 0) =>
    new Intl.NumberFormat("en-US", {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    })
        .format(Number(value))
        .toString();

export const percentageFormatter = (value: number, decimals = 1) => {
    const formattedNumber = new Intl.NumberFormat("en-US", {
        style: "percent",
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    }).format(value);

    const symbol = value > 0 && value !== Infinity ? "+" : "";

    return `${symbol}${formattedNumber}`;
};

export const millionFormatter = (value: number, decimals = 1) => {
    const formattedNumber = new Intl.NumberFormat("en-US", {
        style: "decimal",
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    }).format(value);

    return `${formattedNumber}M`;
};

export const formatters: Record<
    string,
    (value: number, ...args: any[]) => string
> = {
    currency: (value: number, currency: string = "USD") =>
        new Intl.NumberFormat("en-US", { style: "currency", currency }).format(value),
    unit: (value: number) => `${usNumberformatter(value)}`
};

// --------------------------------------------------------
// Dates / time helpers
// --------------------------------------------------------

/**
 * Expects YYYY-MM-DD (date-only).
 * Returns Date in local timezone.
 */
export const convertDateOnlyToDate = (dateString: string) => {
    const [yearStr, monthStr, dayStr] = dateString.split("-");
    const year = Number(yearStr);
    const month = Number(monthStr);
    const day = Number(dayStr);

    // Basic guard (keeps old behavior but avoids NaN dates)
    if (!Number.isFinite(year) || !Number.isFinite(month) || !Number.isFinite(day)) {
        return new Date(NaN);
    }

    return new Date(year, month - 1, day);
};

export const isOutOfWorkingHours = (date: Date, startHour = 9, endHour = 20) => {
    const hours = date.getHours();
    return hours >= endHour || hours < startHour;
};

export const filterPassedTime = (time: Date | string | number) => {
    const currentDate = new Date();
    const selectedDate = new Date(time);

    if (Number.isNaN(selectedDate.getTime())) return false;

    if (currentDate.getTime() >= selectedDate.getTime()) return false;

    if (isOutOfWorkingHours(selectedDate)) return false;

    return true;
};

// --------------------------------------------------------
// Campaign validation (self-contained shape, no UI imports)
// --------------------------------------------------------

export type CampaignChannelsLike = {
    isViber: boolean;
    isSms: boolean;
    fallbackToSMS?: boolean;

    viberSender?: unknown;
    viberMessage?: unknown;

    smsSender?: unknown;
    smsMessage?: unknown;
};

export const validateCampaignChannels = (
    campaign: CampaignChannelsLike,
    showErrorMessage: (message: string) => void
): boolean => {
    // check both channels
    if (!campaign.isViber && !campaign.isSms) {
        showErrorMessage("Campaign doesn't have selected channels.");
        return false;
    }

    // check Viber channel
    if (campaign.isViber) {
        if (!campaign.viberSender) {
            showErrorMessage("Campaign's Viber sender is not defined.");
            return false;
        }

        if (!campaign.viberMessage) {
            showErrorMessage("Campaign's Viber message is not defined.");
            return false;
        }
    }

    // check SMS channel (or fallback)
    if (campaign.isSms || campaign.fallbackToSMS) {
        if (!campaign.smsSender) {
            showErrorMessage("Campaign's SMS sender is not defined.");
            return false;
        }

        if (!campaign.smsMessage) {
            showErrorMessage("Campaign's SMS message is not defined.");
            return false;
        }
    }

    return true;
};

// --------------------------------------------------------
// Media helpers
// --------------------------------------------------------

const IMAGE_EXTENSIONS = [".jpg", ".jpeg", ".png"] as const;
const VIDEO_EXTENSIONS = [".mp4", ".avi"] as const;

const getFileExtension = (path: string) => {
    // strip query/hash
    const clean = path.split(/[?#]/)[0];
    const idx = clean.lastIndexOf(".");
    if (idx === -1) return "";
    return clean.slice(idx).toLowerCase();
};

export const getMediaType = (mediaPath: string): CampaignMediaType => {
    const ext = getFileExtension(mediaPath);

    if (IMAGE_EXTENSIONS.includes(ext as any)) return CampaignMediaType.Image;
    if (VIDEO_EXTENSIONS.includes(ext as any)) return CampaignMediaType.Video;

    throw new Error("Invalid file format.");
};
