// Enums

export enum UserRole {
    Administrator = 1,
    ContentManager = 2
}

export enum CampaignSendingType {
    ScheduledDateTime = 1,
    ByDateOfBirth = 2,
    Immediate = 3
}

export enum CampaignStatus {
    Draft = 1,
    Queued = 2,
    InProgress = 3,
    Done = 4,
    /** Launch accepted; server is building the lead snapshot before the campaign is queued. */
    PreparingLaunch = 5
}

export enum CampaignLeadViberStatus {
    Pending = 1,
    Sent = 2,
    Error = 3
}

export enum CampaignLeadViberStatusCollection {
    NotSent = 0,
    Received = 1,
    Pending = 2,
    Delivered = 3,
    Seen = 4,
    Undelivered = 5,
    Expired = 6,
    Clicked = 7
}

export enum CampaignLeadSMSStatus {
    InvalidUsernameOrPassword = -202,
    InvalidReference = -201,
    ErrorDescription = -200,
    Unknown = -1,
    None = 0,
    Delivered = 1,

    // Legacy typo kept for backward compatibility:
    Undelivired = 2,
    // Preferred correct alias:
    Undelivered = 2,

    InvalidPhone = 3,
    Pending = 5,
    SendingError = 6,
    Blacklisted = 8
}

export enum CampaignMediaType {
    Image = 1,
    Video = 2
}

/** Matches backend <c>CampaignViberContentKind</c>. */
export enum CampaignViberContentKind {
    Text = 0,
    Image = 1,
    Video = 2,
    File = 3,
    Survey = 4
}

export enum LeadGender {
    Male = 1,
    Female = 2
}

export enum SenderType {
    Viber = 1,
    SMS = 2
}

export enum ApiMessageType {
    Viber = 1,
    Sms = 2
}

export enum ApiMessageViberStatus {
    None = 0,
    Received = 1,
    Pending = 2,
    Delivered = 3,
    Seen = 4,
    Undelivered = 5,
    Expired = 6,
    Clicked = 7
}

// --------------------------------------------------------
// Converters / helpers
// --------------------------------------------------------

/**
 * Prefer i18n in UI (next-intl), but if you need a stable value from enum,
 * return a consistent key-like string.
 */
export const convertLeadGenderToString = (gender: LeadGender): string => {
    switch (gender) {
        case LeadGender.Male:
            return "Male";
        case LeadGender.Female:
            return "Female";
        default:
            return "Unknown";
    }
};

/**
 * Converts a string to a numeric enum-like value.
 * Returns undefined if value is not a finite number.
 *
 * NOTE: This does NOT validate that the number exists in a specific enum.
 */
export const convertStringToEnum = <T>(value: string): T | undefined => {
    const n = Number(value);
    return Number.isFinite(n) ? (n as T) : undefined;
};

/**
 * Converts a string to a value of a specific enum object (validated).
 * Works for numeric enums; ignores reverse mappings.
 */
export const convertStringToEnumValue = <E extends Record<string, string | number>>(
    enumObj: E,
    value: string
): E[keyof E] | undefined => {
    const n = Number(value);
    if (!Number.isFinite(n)) return undefined;

    const numericValues = Object.values(enumObj).filter((v): v is number => typeof v === "number");
    return numericValues.includes(n) ? (n as E[keyof E]) : undefined;
};

/**
 * Converts an enum object to array for dropdowns, etc.
 * Filters out numeric keys (reverse mapping in TS numeric enums).
 */
export const enumToArray = <E extends Record<string, string | number>>(enumObj: E) => {
    return Object.keys(enumObj)
        .filter((key) => Number.isNaN(Number(key)))
        .map((key) => ({
            label: key,
            value: enumObj[key] as E[keyof E]
        }));
};
