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

        /// <summary>Comtrade type 220 — extension (with dot) → <c>fileType</c> string in API (e.g. pdf).</summary>
        private static readonly Dictionary<string, string> ViberDocumentExtensionToFileType = new(StringComparer.OrdinalIgnoreCase)
        {
            [".doc"] = "doc", [".docx"] = "docx", [".rtf"] = "rtf", [".dot"] = "dot", [".dotx"] = "dotx",
            [".odt"] = "odt", [".odf"] = "odf", [".fodt"] = "fodt", [".txt"] = "txt", [".info"] = "info",
            [".pdf"] = "pdf", [".xps"] = "xps", [".pdax"] = "pdax", [".eps"] = "eps",
            [".xls"] = "xls", [".xlsx"] = "xlsx", [".ods"] = "ods", [".fods"] = "fods", [".csv"] = "csv",
            [".xlsm"] = "xlsm", [".xltx"] = "xltx"
        };

        public static bool TryGetViberDocumentFileType(string mediaPath, out string fileType)
        {
            fileType = "";
            if (string.IsNullOrWhiteSpace(mediaPath))
            {
                return false;
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
                return false;
            }

            string ext = pathForExt[lastDot..].ToLowerInvariant();
            return ViberDocumentExtensionToFileType.TryGetValue(ext, out fileType!);
        }

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
