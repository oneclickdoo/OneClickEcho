namespace OneClickEcho.Application.Common.Services.ViberService.Request.Enum
{
    public enum ViberSendMessageType : int
    {
        OneWayTextOnly = 106,
        OneWayTextImageButton = 108,
        OneWayTextButton = 109,
        OneWayVideoText = 231,
        OneWayVideoTextButton = 232,
        /// <summary>Video + text + action button: video in <c>MediaUrl</c>, action link in <c>ButtonUrl</c>.</summary>
        OneWayVideoTextActionButton = 233,

        // You always need text
        OneWayImageOnly = 107,
        OneWayVideo = 230,

        // We dont handle files
        OneWayFile = 220,

        // We dont handle two way connection
        TwoWayTextOnly = 206,
        TwoWayImageOnly = 207,
        TwoWayTextImageButton = 208,
        TwoWayTextButton = 209,
        TwoWayTextVideoButton = 210,
    }
}
