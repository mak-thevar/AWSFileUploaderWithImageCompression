using Amazon.S3.Model;
using AWSFileUploaderWithImageCompression.Classes;
using AWSFileUploaderWithImageCompression.Classes.Models;

namespace AWSFileUploaderWithImageCompression.Interfaces
{
    public interface IImageService
    {
        IImageCompressor ImageCompressor { get; set; }
        IS3ImageUploader S3ImageUploader { get; set; }

        Task<ImageServiceResponse> CompressAndUploadImageAsync(Stream sourceImageStream, string fileKeyName = "");
        Task<ImageServiceResponse> CompressWaterMarkAndUploadAsync(Stream sourceImageStream, Stream watermarkImage, string fileKeyName = "");

        Task<ImageServiceResponse> CompressAndUploadImageAsync(string sourceFilePath, string fileKeyName = "");
        Task<ImageServiceResponse> CompressWaterMarkAndUploadAsync(string sourceFilePath, Stream watermarkImage, string fileKeyName = "");
    }
}