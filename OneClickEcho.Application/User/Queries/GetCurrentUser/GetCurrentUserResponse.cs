namespace OneClickEcho.Application.User.Queries.GetCurrentUser;

public record GetCurrentUserResponse(
    string Email,
    ICollection<string> Roles,
    ICollection<GetCompanyDto> Companies
);

public record GetCompanyDto(
    Guid CompanyId,
    string Name,
    DateTime CreatedAt
);