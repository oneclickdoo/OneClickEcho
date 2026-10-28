using OneClickEcho.Domain.Common.Shared;

namespace OneClickEcho.Application.ViberSession;

public interface IViberSessionSender
{
    Task<Result<ViberSessionResponse>> SendAsync(SendViberSessionCommand request, CancellationToken cancellationToken);
}
