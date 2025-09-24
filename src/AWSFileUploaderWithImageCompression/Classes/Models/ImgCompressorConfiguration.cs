using ImageMagick;

namespace AWSFileUploaderWithImageCompression.Models;

public class ImgCompressorConfiguration
{
    public int MaxWidth { get; set; } = 1600;
    public int MaxHeight { get; set; } = 1200;
    public bool MaintainAspectRatio { get; set; } = true;

    /// <summary>
    /// The direction to set the watermark
    /// </summary>
    public Gravity WaterMarkPosition { get; set; } = Gravity.Southeast;

    /// <summary>
    /// From 0 to 10, 10 = 100% transparency and 0 = 0% transparency
    /// </summary>
    public double WaterMarkTransperency { get; set; } = 3.5;
}
