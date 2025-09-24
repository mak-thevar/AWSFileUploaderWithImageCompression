using System;
using System.IO;
using System.Threading.Tasks;
using AWSFileUploaderWithImageCompression.Models;
using ImageMagick;

namespace AWSFileUploaderWithImageCompression;

public class ImageCompressor : IImageCompressor
{
    private ImgCompressorConfiguration serviceConfiguration;

    public ImageCompressor(ImgCompressorConfiguration? serviceConfiguration = null)
    {
        this.serviceConfiguration = serviceConfiguration ?? new ImgCompressorConfiguration();
    }

    public ImageCompressor(Action<ImgCompressorConfiguration>? serviceConfiguration = null)
    {
        var imgCompressorConfig = new ImgCompressorConfiguration();
        serviceConfiguration?.Invoke(imgCompressorConfig);
        this.serviceConfiguration = imgCompressorConfig;
    }

    public void UpdateImageServiceConfiguration(ImgCompressorConfiguration serviceConfiguration)
    {
        this.serviceConfiguration = serviceConfiguration ?? throw new ArgumentNullException(nameof(serviceConfiguration));
    }

    public void UpdateImageServiceConfiguration(Action<ImgCompressorConfiguration> serviceConfiguration)
    {
        ArgumentNullException.ThrowIfNull(serviceConfiguration);
        var imgCompressorConfig = new ImgCompressorConfiguration();
        serviceConfiguration.Invoke(imgCompressorConfig);
        this.serviceConfiguration = imgCompressorConfig;
    }

    public async Task<bool> AddWaterMark(Stream sourceImageStream, Stream waterMarkImageStream, string outputFilePath)
    {
        ArgumentNullException.ThrowIfNull(sourceImageStream);
        ArgumentNullException.ThrowIfNull(waterMarkImageStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFilePath);

        using var watermark = new MagickImage(waterMarkImageStream);
        using var image = new MagickImage(sourceImageStream);

        watermark.Resize(Math.Max(1, image.Width - 10), image.Height * 25 / 100);
        watermark.Evaluate(Channels.Alpha, EvaluateOperator.Divide, serviceConfiguration.WaterMarkTransperency);

        image.Composite(watermark, serviceConfiguration.WaterMarkPosition, CompositeOperator.Over);

        await image.WriteAsync(outputFilePath).ConfigureAwait(false);
        return true;
    }

    public async Task<ImageCompressorResponse> CompressImage(Stream imageStream, string outputFilePath)
    {
        ArgumentNullException.ThrowIfNull(imageStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFilePath);

        using var img = new MagickImage(imageStream);
        var originalHeight = img.Height;
        var originalWidth = img.Width;

        if (img.Height <= serviceConfiguration.MaxHeight && img.Width <= serviceConfiguration.MaxWidth)
        {
            return new ImageCompressorResponse
            {
                ImageCompressionSucccess = false,
                OriginalImageHeight = originalHeight,
                OriginalImageWidth = originalWidth,
                OriginalSizeInBytes = imageStream.Length,
            };
        }

        var imgGeo = new MagickGeometry
        {
            IgnoreAspectRatio = !serviceConfiguration.MaintainAspectRatio,
        };

        if (img.Height > serviceConfiguration.MaxHeight)
        {
            imgGeo.Height = serviceConfiguration.MaxHeight;
        }
        else if (img.Width > serviceConfiguration.MaxWidth)
        {
            imgGeo.Width = serviceConfiguration.MaxWidth;
        }

        img.Resize(imgGeo);

        using var memStream = new MemoryStream();
        await img.WriteAsync(memStream).ConfigureAwait(false);

        memStream.Position = 0;
        var optimizer = new ImageOptimizer();
        var compressionEligible = img.Height >= serviceConfiguration.MaxHeight || img.Width >= serviceConfiguration.MaxWidth;
        var compressionSucceeded = compressionEligible && optimizer.LosslessCompress(memStream);

        memStream.Position = 0;
        var compressedBytes = memStream.ToArray();
        await File.WriteAllBytesAsync(outputFilePath, compressedBytes).ConfigureAwait(false);

        using var outputImage = new MagickImage(compressedBytes);
        return new ImageCompressorResponse
        {
            ImageCompressionSucccess = compressionSucceeded,
            OriginalSizeInBytes = imageStream.Length,
            AfterCompressionSizeInBytes = compressedBytes.LongLength,
            OutputFilePath = outputFilePath,
            OutPutFileStream = new MemoryStream(compressedBytes, writable: false),
            OriginalImageWidth = originalWidth,
            OriginalImageHeight = originalHeight,
            ResizedImageHeight = outputImage.Height,
            ResizedImageWidth = outputImage.Width,
        };
    }

    public async Task<bool> AddWaterMark(string sourceImagePath, string watermarkImagePath, string outputFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceImagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(watermarkImagePath);

        await using var sourceStream = File.OpenRead(sourceImagePath);
        await using var watermarkStream = File.OpenRead(watermarkImagePath);
        return await AddWaterMark(sourceStream, watermarkStream, outputFilePath).ConfigureAwait(false);
    }

    public async Task<ImageCompressorResponse> CompressImage(string sourceImagePath, string outputFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceImagePath);

        await using var sourceStream = File.OpenRead(sourceImagePath);
        return await CompressImage(sourceStream, outputFilePath).ConfigureAwait(false);
    }
}
