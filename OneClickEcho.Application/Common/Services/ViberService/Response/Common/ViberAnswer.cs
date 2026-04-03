namespace OneClickEcho.Application.Common.Services.ViberService.Response.Common
{
    public class ViberAnswer
    {
        public required string MSISDN { get; set; } = string.Empty;

        public required long MessageId { get; set; }

        public required string MessageText { get; set; } = string.Empty;

        public required string MessageToken { get; set; } = string.Empty;

        public required DateTime Received { get; set; }
    }
}
