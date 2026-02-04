using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using OneClickEcho.Domain.NounCaseAggregate.Repositories;

namespace OneClickEcho.Application.NounCase.Queries.GetNounCaseByNominative;

public class GetNounCaseByNominativeHandler(INounCaseRepository nounCaseRepository) : IQueryHandler<GetNounCaseByNominativeQuery, GetNounCaseByNominativeResponse>
{
    private readonly INounCaseRepository _nounCaseRepository = nounCaseRepository;

    public async Task<Result<GetNounCaseByNominativeResponse>> Handle(GetNounCaseByNominativeQuery query,
        CancellationToken cancellationToken)
    {
        Domain.NounCaseAggregate.NounCase? nounCase = await _nounCaseRepository.GetByNominativeAsync(query.Nominative, cancellationToken);

        return nounCase is null
            ? Result.Failure<GetNounCaseByNominativeResponse>(new Error(
                "Request.NotFound",
                $"The NounCase:\"{query.Nominative}\" does not exist."
            ))
            : (Result<GetNounCaseByNominativeResponse>)new GetNounCaseByNominativeResponse(
            nounCase.Id.Value,
            nounCase.Nominative,
            nounCase.Vocative,
            nounCase.CreatedAt
        );
    }
}