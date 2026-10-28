using OneClickEcho.Application.Common.Services.ViberService;

namespace OneClickEcho.Infrastructure.Services.MessageHandling.Viber;

public sealed class ConfiguredViberAccount(string username, string password) : IViberAccount
{
    public bool IsConfigured { get; } = !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
    public string Username { get; } = username;
    public string Password { get; } = password;
}
