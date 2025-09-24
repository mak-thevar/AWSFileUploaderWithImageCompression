using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;
using AWSFileUploaderWithImageCompression.Models;

namespace AWSFileUploaderWithImageCompression;

public class ImageService : IImageService, IAsyncDisposable
{
    private IImageCompressor imageComp;
    private IS3ImageUploader s3Uploader;

    public ImageService(IS3ImageUploader s3ImageUploader, ImgCompressorConfiguration? imgCompressorConfiguration = null)
    {
        ArgumentNullException.ThrowIfNull(s3ImageUploader);

        imageComp = new ImageCompressor(imgCompressorConfiguration);
        s3Uploader = s3ImageUploader;
    }

    public ImageService(IS3ImageUploader s3ImageUploader, Action<ImgCompressorConfiguration>? imgCompressorConfiguration)
    {
        ArgumentNullException.ThrowIfNull(s3ImageUploader);

        imageComp = new ImageCompressor(imgCompressorConfiguration);
        s3Uploader = s3ImageUploader;
    }

    public IImageCompressor ImageCompressor
    {
        get => imageComp;
        set => imageComp = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IS3ImageUploader S3ImageUploader
    {
        get => s3Uploader;
        set => s3Uploader = value ?? throw new ArgumentNullException(nameof(value));
    }

    public async Task<ImageServiceResponse> CompressAndUploadImageAsync(Stream sourceImageStream, string fileKeyName = "")
    {
        ArgumentNullException.ThrowIfNull(sourceImageStream);

        var tmpFile = Path.GetTempFileName();
        try
        {
            var comp = await imageComp.CompressImage(sourceImageStream, tmpFile).ConfigureAwait(false);
            if (!comp.ImageCompressionSucccess)
            {
                throw new InvalidOperationException("Image compression failed.");
            }

            await using var tempStream = File.Open(tmpFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            var uploaderResp = await s3Uploader.UploadAsync(tempStream, key: fileKeyName).ConfigureAwait(false);
            return SendResponse(uploaderResp, comp);
        }
        finally
        {
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }
        }
    }

    public async Task<ImageServiceResponse> CompressAndUploadImageAsync(string sourceFilePath, string fileKeyName = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);

        await using var sourceStream = File.OpenRead(sourceFilePath);
        return await CompressAndUploadImageAsync(sourceStream, fileKeyName).ConfigureAwait(false);
    }

    public async Task<ImageServiceResponse> CompressWaterMarkAndUploadAsync(Stream sourceImageStream, Stream watermarkImage, string fileKeyName = "")
    {
        ArgumentNullException.ThrowIfNull(sourceImageStream);
        ArgumentNullException.ThrowIfNull(watermarkImage);

        var tmpFile = Path.GetTempFileName();
        try
        {
            var watermarkApplied = await imageComp.AddWaterMark(sourceImageStream, watermarkImage, tmpFile).ConfigureAwait(false);
            if (!watermarkApplied)
            {
                throw new InvalidOperationException("Applying watermark failed.");
            }

            await using var tempStream = File.Open(tmpFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            return await CompressAndUploadImageAsync(tempStream, fileKeyName).ConfigureAwait(false);
        }
        finally
        {
            if (File.Exists(tmpFile))
            {
                File.Delete(tmpFile);
            }
        }
    }

    public async Task<ImageServiceResponse> CompressWaterMarkAndUploadAsync(string sourceFilePath, Stream watermarkImage, string fileKeyName = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);
        ArgumentNullException.ThrowIfNull(watermarkImage);

        await using var sourceStream = File.OpenRead(sourceFilePath);
        return await CompressWaterMarkAndUploadAsync(sourceStream, watermarkImage, fileKeyName).ConfigureAwait(false);
    }

    public async Task<ImageServiceResponse> CompressWaterMarkAndUploadAsync(string sourceFilePath, string watermarkImagePath, string fileKeyName = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(watermarkImagePath);

        await using var sourceStream = File.OpenRead(sourceFilePath);
        await using var watermarkStream = File.OpenRead(watermarkImagePath);
        return await CompressWaterMarkAndUploadAsync(sourceStream, watermarkStream, fileKeyName).ConfigureAwait(false);
    }

    private static ImageServiceResponse SendResponse(PutObjectResponse putObjectResponse, ImageCompressorResponse imageCompressorResponse)
    {
        return new ImageServiceResponse
        {
            ImageCompressorResponse = imageCompressorResponse,
            PutObjectResponse = putObjectResponse,
        };
    }

    public ValueTask DisposeAsync()
    {
        return s3Uploader.DisposeAsync();
    }
}
