using System.Text.Json.Serialization;

namespace OneClickEcho.Application.Common.Services.SmsService.Response
{
    public class SendSmsDeliveryResponseDto
    {
        public int StatusVal { get; set; }

        /// <summary>Provider JSON uses <c>Reference</c>; older name <c>ReferenceId</c> kept for compatibility.</summary>
        [JsonPropertyName("Reference")]
        public string ReferenceId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        /// <summary>Human-readable status line from DR API (e.g. error text when <see cref="StatusVal"/> is -200).</summary>
        [JsonPropertyName("Status")]
        public string? StatusText { get; set; }
    }
}
