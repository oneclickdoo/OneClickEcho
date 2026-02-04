using OneClickEcho.Domain.LeadAggregate.Enums;

namespace OneClickEcho.Application.Common.Services;

public interface ICsvReaderService
{
    public Task<List<CsvLeadDto>> CsvToLeads(string filename, CancellationToken cancellationToken = default);
}

public record CsvLeadDto(
    string PhoneNumber,
    string? FirstName,
    string? LastName,
    LeadGender? Gender,
    string? Email,
    DateOnly? DateOfBirth,
    string? City,
    string? State,
    string? Country
);
