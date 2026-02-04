using OneClickEcho.Application.Common.Messaging;

namespace OneClickEcho.Application.NounCase.Queries.GetNounCaseByNominative;

public sealed record GetNounCaseByNominativeQuery(string Nominative) : IQuery<GetNounCaseByNominativeResponse>;