/**
 * Serbia mobile numbers:
 * - Accepts: +3816..., 3816..., 06...
 * - Then operator range and subscriber length rules as defined below.
 *
 * NOTE: This is format validation only (not real number validation).
 */
export const PHONE_NUMBER_REGEX =
    /^(\+3816|3816|06)(([0-6]|[8-9])\d{6,7}|(77|78)\d{5,6})$/;

/**
 * E.164-ish format:
 * - Optional leading "+"
 * - Country code cannot start with 0
 * - Max 15 digits total (E.164 limit)
 */
export const PHONE_NUMBER_REGEX_DIRECT = /^\+?[1-9]\d{1,14}$/;
