namespace OneClickEcho.Application.Common.Services.ViberService.Response.Enum
{
    public enum ViberMessageResponseStatus : int
    {
        MSG_SUCCESS = 0,
        MSG_GENERAL_ERROR = 1,
        MSG_UNAUTHORIZED = 2,
        MSG_NO_CREDIT = 3,
        MSG_WRONG_DESTINATION = 4,
        MSG_WRONG_DISPLAY = 5
    }
}
