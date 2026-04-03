namespace OneClickEcho.Application.NounCase.Queries.GetNounCaseByNominative;

public sealed record GetNounCaseByNominativeResponse(
    Guid Id,
    string Nominative,
    string? Vocative,
    DateTime CreatedAt
);