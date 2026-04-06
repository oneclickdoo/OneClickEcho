/** Mirrors backend RegexHelper.PHONE_NUMBER_REGEX */
const PHONE_NUMBER_REGEX = /^\+?[1-9]\d{1,14}$/;

function stripNoise(raw: string): string {
    return raw.replace(/[\s\-\u00A0()]/g, "").trim();
}

/**
 * Normalizes pasted phone strings (e.g. Serbian 06… → +3816…) and checks E.164-style pattern used by the API.
 */
export function normalizePhoneForImport(raw: string): string | null {
    let s = stripNoise(raw);
    if (!s) return null;

    if (s.startsWith("00") && s.length > 2) {
        s = `+${s.slice(2)}`;
    }

    // Serbian mobile written as 06xxxxxxxxx
    if (/^0[6-9]\d{7,9}$/.test(s)) {
        s = `+381${s.slice(1)}`;
    } else if (/^381[1-9]\d{8,12}$/.test(s)) {
        s = `+${s}`;
    }

    if (!PHONE_NUMBER_REGEX.test(s)) {
        return null;
    }

    return s;
}

export function splitPhoneInputBlock(text: string): string[] {
    const parts = text
        .split(/[\n\r,;]+/)
        .map((p) => p.trim())
        .filter(Boolean);
    return parts;
}

export type ParsedPhoneList = {
    validPhones: string[];
    invalidTokens: string[];
    duplicateCount: number;
};

export function parsePhoneListFromText(text: string): ParsedPhoneList {
    const tokens = splitPhoneInputBlock(text);
    const invalidTokens: string[] = [];
    const seen = new Set<string>();
    const validPhones: string[] = [];
    let duplicateCount = 0;

    for (const token of tokens) {
        const normalized = normalizePhoneForImport(token);
        if (normalized === null) {
            invalidTokens.push(token);
            continue;
        }
        if (seen.has(normalized)) {
            duplicateCount += 1;
            continue;
        }
        seen.add(normalized);
        validPhones.push(normalized);
    }

    return { validPhones, invalidTokens, duplicateCount };
}

const CSV_HEADER = "PhoneNumber,FirstName,LastName,Gender,Email,DateOfBirth,City,State,Country";

export function buildMinimalLeadsCsv(phones: string[]): string {
    const lines = [CSV_HEADER, ...phones.map((p) => `${p},,,,,,,,`)];
    return `${lines.join("\r\n")}\r\n`;
}
