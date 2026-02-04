using OneClickEcho.Application.Common.Messaging;
using OneClickEcho.Domain.Common.Shared;
using System.Text;

namespace OneClickEcho.Application.Lead.Queries.DownloadExampleLeadCsv
{
    public class DownloadExampleLeadCsvHandler : IQueryHandler<DownloadExampleLeadCsvQuery, DownloadExampleLeadCsvResponse>
    {
        public async Task<Result<DownloadExampleLeadCsvResponse>> Handle(DownloadExampleLeadCsvQuery request, CancellationToken cancellationToken)
        {
            // create CSV content
            StringBuilder csvContent = new();

            // append header row
            csvContent.AppendLine("PhoneNumber,FirstName,LastName,Gender,Email,DateOfBirth,City,State,Country");

            // encode CSV content
            byte[] fileBytes = Encoding.UTF8.GetBytes(csvContent.ToString());

            DownloadExampleLeadCsvResponse response = new(fileBytes);

            return await Task.FromResult(response);
        }
    }
}
