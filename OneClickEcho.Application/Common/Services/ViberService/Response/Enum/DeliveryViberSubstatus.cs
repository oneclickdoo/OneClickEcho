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
        SRVC_BAD_LABEL = 18
    }
}
