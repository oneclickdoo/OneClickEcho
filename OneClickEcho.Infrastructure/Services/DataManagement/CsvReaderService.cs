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
            char delimiter = DetectDelimiter(path);

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter.ToString(),
                HeaderValidated = null,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.Trim().TrimStart('\uFEFF'),
            };

            using StreamReader reader = new(path);
            using CsvReader csv = new(reader, config);
            csv.Context.RegisterClassMap<CsvLeadDtoMap>();

            return Task.FromResult(csv.GetRecords<CsvLeadDto>().ToList());
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to process CSV file.", ex);
        }
    }

    private static char DetectDelimiter(string path)
    {
        string? header;
        string? sample;
        using (StreamReader peekReader = new(path))
        {
            header = peekReader.ReadLine();
            sample = peekReader.ReadLine();
        }

        header ??= string.Empty;
        sample ??= string.Empty;
        int headerSemi = header.Count(c => c == ';');
        int headerComma = header.Count(c => c == ',');
        if (headerSemi > 0 || headerComma > 0)
        {
            return headerSemi > headerComma ? ';' : ',';
        }

        int sampleSemi = sample.Count(c => c == ';');
        int sampleComma = sample.Count(c => c == ',');
        if (sampleSemi > 0 || sampleComma > 0)
        {
            return sampleSemi >= sampleComma ? ';' : ',';
        }

        return ',';
    }

    private sealed class CsvLeadDtoMap : ClassMap<CsvLeadDto>
    {
        public CsvLeadDtoMap()
        {
            Parameter("PhoneNumber").Name("PhoneNumber");
            Parameter("FirstName").Name("FirstName").Optional();
            Parameter("LastName").Name("LastName").Optional();
            Parameter("Gender").Name("Gender").Optional();
            Parameter("Email").Name("Email").Optional();
            Parameter("DateOfBirth").Name("DateOfBirth").Optional();
            Parameter("City").Name("City").Optional();
            Parameter("State").Name("State").Optional();
            Parameter("Country").Name("Country").Optional();
        }
    }
}
