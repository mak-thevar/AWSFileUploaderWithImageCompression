using System.IO;
using System.Text.Json.Serialization;

namespace AWSFileUploaderWithImageCompression.Models;

public class ImageCompressorResponse
{
    /// <summary>
    /// Whether the lossless compression was successful; if false then the image has been just resized (if the image had exceeded the maxWidth | maxHeight).
    /// </summary>
    public bool ImageCompressionSucccess { get; set; }
    public long OriginalSizeInBytes { get; set; }
    public int OriginalImageWidth { get; set; }
    public int OriginalImageHeight { get; set; }
    public long AfterCompressionSizeInBytes { get; set; }

    public int ResizedImageHeight { get; set; }
    public int ResizedImageWidth { get; set; }

    public string OutputFilePath { get; set; } = string.Empty;

    [JsonIgnore]
    public Stream? OutPutFileStream { get; set; }
}
