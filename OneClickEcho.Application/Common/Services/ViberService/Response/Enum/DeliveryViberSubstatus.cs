namespace OneClickEcho.Application.Common.Services.ViberService.Response.Enum
{
    public enum DeliveryViberSubstatus : int
    {
        SRVC_SUCCESS = 0,
        SRVC_INTERNAL_FAILURE = 1,
        SRVC_BAD_SERVICE_ID = 2,
        SRVC_BAD_DATA = 3,
        SRVC_BLOCKED_MESSAGE_TYPE = 4,
        SRVC_BAD_MESSAGE_TYPE = 5,
        SRVC_BAD_PARAMETERS = 6,
        SRVC_TIMEOUT = 7,
        SRVC_USER_BLOCKED = 8,
        SRVC_NOT_VIBER_USER = 9,
        SRVC_NO_SUITABLE_DEVICE = 10,
        SRVC_UNAUTHORIZED_IP = 11,
        SRVC_ALREADY_SENT = 12,
        SRVC_NOT_PERMITTED = 13,
        SRVC_BILLING_FAILURE = 14,
        SRVC_NO_MORE_MESSAGES = 17,
        SRVC_BAD_LABEL = 18,

        // Comtrade Viber API (extended; see CT Viber API.pdf)
        SRVC_INVALID_TTL = 20,
        SRVC_WAIT_FOR_USER_RESPONSE = 21,
        SRVC_INVALID_PHONE_NUMBER = 27,
        SRVC_FILE_NOT_PERMITTED = 28,
        SRVC_BAD_FILE_NAME_LENGTH = 29,
        SRVC_BAD_THUMBNAIL = 30,
        SRVC_BAD_FILE_SIZE = 31,
        SRVC_BAD_DURATION = 32,
        TEMPLATE_NOT_FOUND = 38,
        TEMPLATE_VALIDATION_ERROR = 39,
        SRVC_SURVEY_VALIDATION_ERROR = 40,
        SRVC_CAROUSEL_VALIDATION_ERROR = 41
    }
}
