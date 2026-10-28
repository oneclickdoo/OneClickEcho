namespace OneClickEcho.Application.Common.Services.ViberService;

public interface IViberAccount
{
    bool IsConfigured { get; }
    string Username { get; }
    string Password { get; }
}
