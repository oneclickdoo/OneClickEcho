using CsvHelper;
using CsvHelper.Configuration;
using OneClickEcho.Application.Common.Services;
using System.Globalization;

namespace OneClickEcho.Infrastructure.Services.DataManagement;

public class CsvReaderService : ICsvReaderService
{
    public Task<List<CsvLeadDto>> CsvToLeads(string filename, CancellationToken cancellationToken = default)
    {
        try
        {
            string path = Path.Combine(FileStorageService.UploadPath, filename);
            string? firstLine;
            using (StreamReader peekReader = new(path))
            {
                firstLine = peekReader.ReadLine();
            }

            char delimiter = firstLine is not null && firstLine.Contains(';') ? ';' : ',';

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter.ToString()
            };

            using StreamReader reader = new(path);
            using CsvReader csv = new(reader, config);

            IEnumerable<CsvLeadDto> records = csv.GetRecords<CsvLeadDto>();

            return Task.FromResult(new List<CsvLeadDto>(records));
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to process CSV file.", ex);
        }
    }
}