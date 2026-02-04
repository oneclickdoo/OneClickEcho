namespace OneClickEcho.Domain.CampaignLeadAggregate.Enums
{
    public enum CampaignLeadSMSStatus : short
    {
        InvalidUsernameOrPassword = -202,
        InvalidReference = -201,
        ErrorDescription = -200,
        Unknown = -1,
        None = 0,
        Delivered = 1,
        Undelivired = 2,
        InvalidPhone = 3,
        Pending = 5,
        SendingError = 6,
        Blacklisted = 8
    }
}
