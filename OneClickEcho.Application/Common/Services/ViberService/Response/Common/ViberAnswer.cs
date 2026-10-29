using System.Text.Json.Serialization;

namespace OneClickEcho.Application.Common.Services.ViberService.Response.Common
{
    public class ViberAnswer
    {
        public required string MSISDN { get; set; } = string.Empty;

        public required long MessageId { get; set; }

        public required string MessageText { get; set; } = string.Empty;

        public string? MessageToken { get; set; }

        /// <summary>Comtrade sends <c>/Date(milliseconds+offset)/</c>, not an ISO timestamp.</summary>
        [JsonConverter(typeof(ComtradeDateTimeJsonConverter))]
        public required DateTime Received { get; set; }
    }
}
