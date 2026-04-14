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
            if (string.IsNullOrWhiteSpace(mediaPath))
            {
                throw new Exception("Invalid file format.");
            }

            string pathForExt = mediaPath.Trim();
            if (pathForExt.Contains("://", StringComparison.Ordinal))
            {
                try
                {
                    var uri = new Uri(pathForExt, UriKind.Absolute);
                    pathForExt = uri.AbsolutePath;
                }
                catch (UriFormatException)
                {
                    int q = pathForExt.IndexOf('?', StringComparison.Ordinal);
                    if (q >= 0)
                    {
                        pathForExt = pathForExt[..q];
                    }

                    int h = pathForExt.IndexOf('#', StringComparison.Ordinal);
                    if (h >= 0)
                    {
                        pathForExt = pathForExt[..h];
                    }
                }
            }
            else
            {
                int q = pathForExt.IndexOf('?', StringComparison.Ordinal);
                if (q >= 0)
                {
                    pathForExt = pathForExt[..q];
                }
            }

            int lastDot = pathForExt.LastIndexOf('.');
            if (lastDot < 0 || lastDot >= pathForExt.Length - 1)
            {
                throw new Exception("Invalid file format.");
            }

            string mediaExtension = pathForExt[lastDot..].ToLowerInvariant();

            return IMAGE_EXTENSIONS.Contains(mediaExtension)
                ? CampaignMediaType.Image
                : VIDEO_EXTENSIONS.Contains(mediaExtension) ? CampaignMediaType.Video : throw new Exception("Invalid file format.");
        }
    }
}
