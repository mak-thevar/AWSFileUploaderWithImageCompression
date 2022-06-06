using AWSFileUploaderWithImageCompression.Models;

namespace AWSFileUploaderWithImageCompression
{
    public interface IImageCompressor
    {
        Task<bool> AddWaterMark(Stream sourceImageStream, Stream waterMarkImageStream, string outputFilePath);
        Task<bool> AddWaterMark(string sourceImagePath, string watermarkImagePath, string outputFilePath);
        Task<ImageCompressorResponse> CompressImage(Stream imageStream, string outputFilePath);
        Task<ImageCompressorResponse> CompressImage(string sourceImagePath, string outputFilePath);
        void UpdateImageServiceConfiguration(ImgCompressorConfiguration serviceConfiguration);
        void UpdateImageServiceConfiguration(Action<ImgCompressorConfiguration> serviceConfiguration);
    }
}