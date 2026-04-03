namespace OneClickEcho.Application.Common.Helpers
{
    public enum CampaignMediaType : short
    {
        Image = 1,
        Video = 2
    }

    public class MediaHelper
    {
        private static readonly string[] IMAGE_EXTENSIONS = [".jpg", ".jpeg", ".png"];

        private static readonly string[] VIDEO_EXTENSIONS = [".mp4", ".avi"];

        public static CampaignMediaType GetMediaType(string mediaPath)
        {
            // get file extension
            string[] mediaPathSubstrings = mediaPath.Split('.');

            string mediaExtension = $".{mediaPathSubstrings[^1]}";

            // check media type
            return IMAGE_EXTENSIONS.Contains(mediaExtension)
                ? CampaignMediaType.Image
                : VIDEO_EXTENSIONS.Contains(mediaExtension) ? CampaignMediaType.Video : throw new Exception("Invalid file format.");
        }
    }
}
