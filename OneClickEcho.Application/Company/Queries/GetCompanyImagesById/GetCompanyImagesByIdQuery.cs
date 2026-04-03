using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.Company.Queries.GetCompanyImagesById;

public record GetCompanyImagesByIdQuery(Guid CompanyId) : IQuery<GetCompanyImagesByIdResponse>;