// Tremor Raw cx [v0.0.0]

import clsx, { type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

import { CampaignMediaType, CampaignViberContentKind } from "@/lib/enums";

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
// Media helpers (used by campaign validation)
// --------------------------------------------------------

const IMAGE_EXTENSIONS = [".jpg", ".jpeg", ".png"] as const;
const VIDEO_EXTENSIONS = [".mp4", ".avi"] as const;

const VIBER_DOCUMENT_EXTENSION_TO_FILE_TYPE: Record<string, string> = {
    ".doc": "doc",
    ".docx": "docx",
    ".rtf": "rtf",
    ".dot": "dot",
    ".dotx": "dotx",
    ".odt": "odt",
    ".odf": "odf",
    ".fodt": "fodt",
    ".txt": "txt",
    ".info": "info",
    ".pdf": "pdf",
    ".xps": "xps",
    ".pdax": "pdax",
    ".eps": "eps",
    ".xls": "xls",
    ".xlsx": "xlsx",
    ".ods": "ods",
    ".fods": "fods",
    ".csv": "csv",
    ".xlsm": "xlsm",
    ".xltx": "xltx"
};

function getMediaPathExtensionLower(mediaPath: string): string {
    let path = mediaPath.trim();
    try {
        if (path.includes("://")) {
            path = new URL(path).pathname;
        }
    } catch {
        /* relative path or invalid URL */
    }
    const q = path.indexOf("?");
    if (q >= 0) {
        path = path.slice(0, q);
    }
    const h = path.indexOf("#");
    if (h >= 0) {
        path = path.slice(0, h);
    }
    const lastDot = path.lastIndexOf(".");
    if (lastDot < 0 || lastDot >= path.length - 1) {
        return "";
    }
    return path.slice(lastDot).toLowerCase();
}

/** Comtrade type 220 file category (e.g. pdf), or null if not an allowed document path/URL. */
export function tryGetViberDocumentFileTypeFromPath(mediaPath: string): string | null {
    const ext = getMediaPathExtensionLower(mediaPath);
    return VIBER_DOCUMENT_EXTENSION_TO_FILE_TYPE[ext] ?? null;
}

/** Path or absolute URL; extension is taken from the URL path (before ?/#). */
export const getMediaType = (mediaPath: string): CampaignMediaType => {
    const ext = getMediaPathExtensionLower(mediaPath);
    if (IMAGE_EXTENSIONS.includes(ext as any)) {
        return CampaignMediaType.Image;
    }
    if (VIDEO_EXTENSIONS.includes(ext as any)) {
        return CampaignMediaType.Video;
    }
    throw new Error("Invalid file format.");
};

// --------------------------------------------------------
// Campaign validation (self-contained shape, no UI imports)
// --------------------------------------------------------

const VIBER_MAX_VIDEO_DURATION_SECONDS = 600;

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
    viberContentKind?: unknown;
    viberSurveyOptionsJson?: unknown;
    isViberReceivable?: unknown;

    smsSender?: unknown;
    smsMessage?: unknown;
};

function resolveClientViberKind(campaign: CampaignChannelsLike): CampaignViberContentKind {
    const raw = campaign.viberContentKind;
    let k =
        typeof raw === "number" && Number.isFinite(raw)
            ? (raw as CampaignViberContentKind)
            : CampaignViberContentKind.Text;

    if (k !== CampaignViberContentKind.Text) {
        return k;
    }

    const surveyJson = campaign.viberSurveyOptionsJson;
    if (typeof surveyJson === "string" && surveyJson.trim()) {
        try {
            const arr = JSON.parse(surveyJson) as unknown;
            if (Array.isArray(arr)) {
                const opts = arr
                    .filter((x): x is string => typeof x === "string")
                    .map((s) => s.trim())
                    .filter((s) => s.length > 0);
                if (opts.length >= 2) {
                    return CampaignViberContentKind.Survey;
                }
            }
        } catch {
            /* ignore */
        }
    }

    const media = campaign.viberMedia;
    if (typeof media === "string" && media.trim()) {
        const docFt = tryGetViberDocumentFileTypeFromPath(media);
        if (docFt) {
            return CampaignViberContentKind.File;
        }
        try {
            return getMediaType(media) === CampaignMediaType.Image
                ? CampaignViberContentKind.Image
                : CampaignViberContentKind.Video;
        } catch {
            /* ignore */
        }
    }

    return CampaignViberContentKind.Text;
}

/** Same video asset as campaign media (full string match or same file name in path/URL). Mirrors backend <c>VideoButtonUrlMatchesMedia</c>. */
export function videoButtonUrlPointsToSameVideo(mediaPath: string, buttonUrl: string): boolean {
    const m = mediaPath.trim();
    const b = buttonUrl.trim();
    if (!m || !b) {
        return false;
    }
    if (m.toLowerCase() === b.toLowerCase()) {
        return true;
    }
    const fileName = (s: string): string => {
        try {
            if (s.includes("://")) {
                const pathOnly = new URL(s).pathname;
                const seg = pathOnly.split("/").filter(Boolean);
                return seg.length ? seg[seg.length - 1]! : "";
            }
        } catch {
            /* ignore */
        }
        const clean = s.split(/[?#]/)[0] ?? s;
        const parts = clean.replace(/\\/g, "/").split("/").filter(Boolean);
        return parts.length ? parts[parts.length - 1]! : "";
    };
    const fm = fileName(m);
    const fb = fileName(b);
    return fm.length > 0 && fb.length > 0 && fm.toLowerCase() === fb.toLowerCase();
}

/** Promotional Viber: video only (Comtrade type 230) — empty message, no button URL, .mp4/.avi media. */
function isPromoViberVideoOnly(campaign: CampaignChannelsLike): boolean {
    const media = campaign.viberMedia;
    const button = campaign.viberButtonUrl;
    const msg = campaign.viberMessage;
    if (typeof media !== "string" || !media) {
        return false;
    }
    if (typeof button === "string" && button.trim().length > 0) {
        if (!videoButtonUrlPointsToSameVideo(media, button)) {
            return false;
        }
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

/** Comtrade 233: video in MediaUrl, separate action in ButtonUrl (not the same asset as <c>viberMedia</c>). */
function isViberVideoType233Layout(campaign: CampaignChannelsLike): boolean {
    if (!campaignHasViberVideoMedia(campaign)) {
        return false;
    }
    const btn = campaign.viberButtonUrl;
    const media = campaign.viberMedia;
    if (typeof btn !== "string" || !btn.trim() || typeof media !== "string" || !media.trim()) {
        return false;
    }
    return !videoButtonUrlPointsToSameVideo(media, btn);
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

        const kind = resolveClientViberKind(campaign);

        if (kind === CampaignViberContentKind.Survey) {
            const msg = typeof campaign.viberMessage === "string" ? campaign.viberMessage : "";
            if (!msg.trim()) {
                showErrorMessage("Viber survey requires intro message text.");
                return false;
            }
            if (!campaign.isViberReceivable) {
                showErrorMessage("Survey campaigns require “Enable responses”.");
                return false;
            }
            const raw = campaign.viberSurveyOptionsJson;
            if (typeof raw !== "string" || !raw.trim()) {
                showErrorMessage("Survey requires 2–5 options.");
                return false;
            }
            let arr: unknown;
            try {
                arr = JSON.parse(raw);
            } catch {
                showErrorMessage("Survey options are not valid JSON.");
                return false;
            }
            if (!Array.isArray(arr)) {
                showErrorMessage("Survey options must be a JSON array.");
                return false;
            }
            const opts = arr
                .filter((x): x is string => typeof x === "string")
                .map((s) => s.replace(/\r\n|\r|\n|\t/g, " ").trim())
                .filter((s) => s.length > 0);
            if (opts.length < 2 || opts.length > 5) {
                showErrorMessage("Survey requires between 2 and 5 non-empty options.");
                return false;
            }
            if (opts.some((o) => o.length > 50)) {
                showErrorMessage("Each survey option must be at most 50 characters.");
                return false;
            }
            const unique = new Set(opts);
            if (unique.size !== opts.length) {
                showErrorMessage("Survey options must be unique (no duplicate answers).");
                return false;
            }
        } else if (kind === CampaignViberContentKind.File) {
            const media = campaign.viberMedia;
            if (typeof media !== "string" || !media.trim() || !tryGetViberDocumentFileTypeFromPath(media)) {
                showErrorMessage(
                    "Viber file message requires a document (.pdf, Office formats, etc.) via upload or URL."
                );
                return false;
            }
        } else if (kind === CampaignViberContentKind.Image) {
            const media = campaign.viberMedia;
            if (typeof media !== "string" || !media.trim()) {
                showErrorMessage("Viber image message requires an image file or URL.");
                return false;
            }
            try {
                if (getMediaType(media) !== CampaignMediaType.Image) {
                    showErrorMessage("Viber image message must use an image (.jpg / .png).");
                    return false;
                }
            } catch {
                showErrorMessage("Invalid image media URL or path.");
                return false;
            }
            const btn = campaign.viberButtonUrl;
            const hasBtn = typeof btn === "string" && btn.trim().length > 0;
            if (hasBtn) {
                const msg = typeof campaign.viberMessage === "string" ? campaign.viberMessage : "";
                if (!msg.trim()) {
                    showErrorMessage("Viber image with a button requires message text.");
                    return false;
                }
            }
        } else if (kind === CampaignViberContentKind.Video) {
            if (!campaignHasViberVideoMedia(campaign)) {
                showErrorMessage("Viber video campaign requires a video file or URL (.mp4 / .avi).");
                return false;
            }
            if (!campaign.viberVideoThumbnail || typeof campaign.viberVideoThumbnail !== "string") {
                showErrorMessage("Viber video requires a thumbnail image.");
                return false;
            }
            if (!isViberVideoType233Layout(campaign)) {
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
                        `Viber accepts videos up to ${VIBER_MAX_VIDEO_DURATION_SECONDS} seconds. Shorten the file or pick another.`
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
            }
        } else {
            if (campaignHasViberVideoMedia(campaign) && !isViberVideoType233Layout(campaign)) {
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
                        `Viber accepts videos up to ${VIBER_MAX_VIDEO_DURATION_SECONDS} seconds. Shorten the file or pick another.`
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
