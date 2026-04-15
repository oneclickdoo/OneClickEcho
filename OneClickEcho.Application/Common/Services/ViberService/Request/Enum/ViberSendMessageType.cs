namespace OneClickEcho.Application.Common.Services.ViberService.Request.Enum
{
    public enum ViberSendMessageType : int
    {
        OneWayTextOnly = 106,
        OneWayTextImageButton = 108,
        OneWayTextButton = 109,
        /// <summary>Comtrade <b>231</b>: video URL in <c>ButtonUrl</c>, <c>MessageText</c>, <c>Thumbnail</c>, <c>Duration</c>, <c>FileSize</c>.</summary>
        OneWayVideoText = 231,
        /// <summary>Comtrade <b>232</b>: video URL in <c>ButtonUrl</c>, <c>MessageText</c>, <c>ButtonCaption</c>, <c>Thumbnail</c>, <c>Duration</c>, <c>FileSize</c>.</summary>
        OneWayVideoTextButton = 232,
        /// <summary>Comtrade <b>233</b>: video in <c>MediaUrl</c>, action in <c>ButtonUrl</c>, <c>MessageText</c>, <c>ButtonCaption</c>, <c>Thumbnail</c>.</summary>
        OneWayVideoTextActionButton = 233,

        // You always need text
        OneWayImageOnly = 107,
        /// <summary>Comtrade <b>230</b>: video-only — <c>ButtonUrl</c> = video, <c>Thumbnail</c>, <c>Duration</c>, <c>FileSize</c> (no <c>MessageText</c>).</summary>
        OneWayVideo = 230,

        /// <summary>File only (Comtrade 220): <c>ButtonUrl</c> = file URL, <c>FileType</c>, <c>NameOfFile</c>.</summary>
        OneWayFile = 220,

        /// <summary>Survey / list (Comtrade 801).</summary>
        OneWaySurveyList = 801,

        // We dont handle two way connection
        TwoWayTextOnly = 206,
        TwoWayImageOnly = 207,
        TwoWayTextImageButton = 208,
        TwoWayTextButton = 209,
        TwoWayTextVideoButton = 210,
    }
}
