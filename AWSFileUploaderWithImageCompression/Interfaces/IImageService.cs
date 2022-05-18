using Amazon.S3.Model;
using AWSFileUploaderWithImageCompression.Classes;

namespace AWSFileUploaderWithImageCompression.Interfaces
{
    public interface IImageService
    {
        IImageCompressor ImageCompressor { get; set; }
        IS3ImageUploader S3ImageUploader { get; set; }

        Task<PutObjectResponse> CompressAndUploadImageAsync(Stream sourceImageStream, string fileKeyName = "");
        Task<PutObjectResponse> CompressWaterMarkAndUploadAsync(Stream sourceImageStream, Stream watermarkImage, string fileKeyName = "");

        Task<PutObjectResponse> CompressAndUploadImageAsync(string sourceFilePath, string fileKeyName = "");
        Task<PutObjectResponse> CompressWaterMarkAndUploadAsync(string sourceFilePath, Stream watermarkImage, string fileKeyName = "");
    }
}