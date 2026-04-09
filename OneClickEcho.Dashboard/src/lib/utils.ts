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
];

// Tremor Raw focusRing [v0.0.1]

export const focusRing = [
    // base
    "outline outline-offset-2 outline-0 focus-visible:outline-2",
    // outline color
    "outline-indigo-500 dark:outline-indigo-500"
];

// Tremor Raw hasErrorInput [v0.0.1]

export const hasErrorInput = [
    // base
    "ring-2",
    // border color
    "border-red-500 dark:border-red-700",
    // ring color
    "ring-red-200 dark:ring-red-700/30"
];

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

const VIBER_MAX_VIDEO_DURATION_SECONDS = 900;

export type CampaignChannelsLike = {
    isViber: boolean;
    isSms: boolean;
    fallbackToSMS?: boolean;

    viberSender?: unknown;
    viberMessage?: unknown;
    viberMedia?: unknown;
    viberButtonUrl?: unknown;
    viberVideoThumbnail?: unknown;
    viberVideoDuration?: unknown;
    viberFileSize?: unknown;

    smsSender?: unknown;
    smsMessage?: unknown;
};

/** Promotional Viber: video only (Comtrade type 230) — empty message, no button URL, .mp4/.avi media. */
function isPromoViberVideoOnly(campaign: CampaignChannelsLike): boolean {
    const media = campaign.viberMedia;
    const button = campaign.viberButtonUrl;
    const msg = campaign.viberMessage;
    if (typeof media !== "string" || !media) {
        return false;
    }
    if (typeof button === "string" && button.trim().length > 0) {
        return false;
    }
    if (typeof msg === "string" && msg.trim().length > 0) {
        return false;
    }
    try {
        return getMediaType(media) === CampaignMediaType.Video;
    } catch {
        return false;
    }
}

function campaignHasViberVideoMedia(campaign: CampaignChannelsLike): boolean {
    const media = campaign.viberMedia;
    if (typeof media !== "string" || !media) {
        return false;
    }
    try {
        return getMediaType(media) === CampaignMediaType.Video;
    } catch {
        return false;
    }
}

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

        if (campaignHasViberVideoMedia(campaign)) {
            const d = campaign.viberVideoDuration;
            const sz = campaign.viberFileSize;
            if (typeof d !== "number" || !Number.isFinite(d) || d < 1) {
                showErrorMessage(
                    "Viber video requires duration in seconds. Open Messaging, wait for the preview to show length, then Save."
                );
                return false;
            }
            if (d > VIBER_MAX_VIDEO_DURATION_SECONDS) {
                showErrorMessage(
                    `Viber accepts videos up to ${VIBER_MAX_VIDEO_DURATION_SECONDS} seconds (15 min). Shorten the file or pick another.`
                );
                return false;
            }
            if (typeof sz !== "number" || !Number.isFinite(sz) || sz < 1) {
                showErrorMessage("Viber video is missing file size metadata. Re-upload the video and save.");
                return false;
            }
        }

        if (isPromoViberVideoOnly(campaign)) {
            if (!campaign.viberVideoThumbnail || typeof campaign.viberVideoThumbnail !== "string") {
                showErrorMessage(
                    "Promotional video-only Viber requires a thumbnail image (upload video, then thumbnail)."
                );
                return false;
            }
        } else if (
            typeof campaign.viberMessage !== "string" ||
            !campaign.viberMessage ||
            !String(campaign.viberMessage).trim()
        ) {
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
