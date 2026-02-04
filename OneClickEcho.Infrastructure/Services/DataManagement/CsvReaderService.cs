using CsvHelper;
using OneClickEcho.Application.Common.Services;
using System.Globalization;

namespace OneClickEcho.Infrastructure.Services.DataManagement;

public class CsvReaderService : ICsvReaderService
{
    public Task<List<CsvLeadDto>> CsvToLeads(string filename, CancellationToken cancellationToken = default)
    {
        try
        {
            // read file
            using StreamReader reader = new(Path.Combine(FileStorageService.UploadPath, filename));

            using CsvReader csv = new(reader, CultureInfo.InvariantCulture);

            IEnumerable<CsvLeadDto> records = csv.GetRecords<CsvLeadDto>();

            return Task.FromResult(new List<CsvLeadDto>(records));
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to process CSV file.", ex);
        }
    }
}