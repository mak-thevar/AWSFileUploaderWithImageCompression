using AWSFileUploaderWithImageCompression.Models;

namespace AWSFileUploaderWithImageCompression
{
    public interface IImageService
    {
        IImageCompressor ImageCompressor { get; set; }
        IS3ImageUploader S3ImageUploader { get; set; }

        Task<ImageServiceResponse> CompressAndUploadImageAsync(Stream sourceImageStream, string fileKeyName = "");
        Task<ImageServiceResponse> CompressWaterMarkAndUploadAsync(Stream sourceImageStream, Stream watermarkImage, string fileKeyName = "");

        Task<ImageServiceResponse> CompressAndUploadImageAsync(string sourceFilePath, string fileKeyName = "");
        Task<ImageServiceResponse> CompressWaterMarkAndUploadAsync(string sourceFilePath, Stream watermarkImage, string fileKeyName = "");
        Task<ImageServiceResponse> CompressWaterMarkAndUploadAsync(string sourceFilePath, string watermarkImagePath, string fileKeyName = "");
    }
}