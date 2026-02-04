using OneClickEcho.Application.Common.Services.ViberService.Request.Enum;

namespace OneClickEcho.Application.Common.Services.ViberService.Request.Common
{
    public class ViberMessage
    {
        // button caption
        public string? ButtonCaption { get; set; }

        // url pointing to web page
        public string? ButtonUrl { get; set; }

        // registered sender
        public string Display { get; set; } = string.Empty;

        // duration of video
        public int? Duration { get; set; }

        // size of video or file
        // maximum file size is 200 MB
        public int? FileSize { get; set; }

        // possible types are:
        // .doc, .docx, .rtf, .dot, .dotx, .odt ,odf, .fodt, .txt, .info, .pdf, .xps, .pdax, .eps,
        // .xls, .xlsx, .ods, .fods, .csv, .xlsm, .xltx
        public string? FileType { get; set; }

        // url pointing to image on web
        public string? ImageUrl { get; set; }

        // services are obligated to state the type of the message
        // possible values - promotion / transaction
        public string Label { get; set; } = "promotion";

        // phone number in international format
        // example: +381...
        public string MSISDN { get; set; } = string.Empty;

        // unique message identifier used as correlator for delivery report
        public long MessageId { get; set; }

        // message text up to 1000 characters (Unicode supported)
        public string? MessageText { get; set; }

        public ViberSendMessageType MessageType { get; set; }

        // includes the name of file and extension (example.pdf)
        public string? NameOfFile { get; set; }

        // value from 0-255 range which specifies message processing priority
        // messages with lower priority will be processed first
        // general recommendation for comertial or promotional is 255
        public byte Priority { get; set; } = 255;

        // free text
        // reports can be later generated based on tags.
        // @NOTE: The reports will not be available for the first stage.
        public string? Tag { get; set; }

        // the opening image of the video
        public string? Thumbnail { get; set; }

        // validity time in seconds
        public int Validity { get; set; }
    }
}
