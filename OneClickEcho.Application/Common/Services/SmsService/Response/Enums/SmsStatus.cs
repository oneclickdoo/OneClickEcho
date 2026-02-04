namespace OneClickEcho.Application.Common.Services.SmsService.Response.Enums
{
    public enum SmsStatus
    {
        Success = 0,
        InvalidPhoneNumber = -1,
        ErrorDescription = -3,
        // invalid username, pwd or sender (Serbia)
        InvalidUsernameSerbia = -10,
        // can't send message (DB error)
        CantSendMessageSerbia = -11,
        // invalid username, pwd or sender (Bosnia)
        InvalidUsernameBosnia = -20,
        // can't send message (DB error)
        CantSendMessageBosnia = -21,
        // invalid username, pwd or sender (non Serbia and Bosnia)
        InvalidUsername = -30,
        // can't send message (DB error)
        CantSendMessage = -31
    }
}
