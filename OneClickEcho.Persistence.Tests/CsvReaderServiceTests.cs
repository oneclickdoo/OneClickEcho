using OneClickEcho.Infrastructure.Services.DataManagement;

namespace OneClickEcho.Persistence.Tests;

public class CsvReaderServiceTests
{
    [Fact]
    public async Task CsvToLeads_accepts_phone_only_with_trailing_semicolons()
    {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), FileStorageService.UploadPath);
        Directory.CreateDirectory(folder);
        string name = $"leads-{Guid.NewGuid():N}.csv";
        string path = Path.Combine(folder, name);
        await File.WriteAllTextAsync(path, """
            PhoneNumber
            381677599826;;;;;;;;
            381659980412;;;;;;;;
            """);

        try
        {
            List<OneClickEcho.Application.Common.Services.CsvLeadDto> rows =
                await new CsvReaderService().CsvToLeads(name);

            Assert.Equal(2, rows.Count);
            Assert.Equal("381677599826", rows[0].PhoneNumber);
            Assert.Null(rows[0].FirstName);
            Assert.Null(rows[0].Gender);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task CsvToLeads_accepts_full_comma_header()
    {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), FileStorageService.UploadPath);
        Directory.CreateDirectory(folder);
        string name = $"leads-{Guid.NewGuid():N}.csv";
        string path = Path.Combine(folder, name);
        await File.WriteAllTextAsync(path, """
            PhoneNumber,FirstName,LastName,Gender,Email,DateOfBirth,City,State,Country
            381677599826,Maja,,,,,
            """);

        try
        {
            List<OneClickEcho.Application.Common.Services.CsvLeadDto> rows =
                await new CsvReaderService().CsvToLeads(name);

            Assert.Single(rows);
            Assert.Equal("381677599826", rows[0].PhoneNumber);
            Assert.Equal("Maja", rows[0].FirstName);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
