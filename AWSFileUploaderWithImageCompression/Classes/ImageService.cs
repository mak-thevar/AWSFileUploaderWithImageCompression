using Amazon.S3.Model;
using AWSFileUploaderWithImageCompression.Classes.Models;
using AWSFileUploaderWithImageCompression.Interfaces;

namespace AWSFileUploaderWithImageCompression.Classes
{
    public class ImageService : IImageService
    {
        private IImageCompressor imageComp;
        private IS3ImageUploader s3Uploader;

        public ImageService(IS3ImageUploader s3ImageUploader, ImgCompressorConfiguration? imgCompressorConfiguration = null)
        {
            this.imageComp = new ImageCompressor(imgCompressorConfiguration);
            this.s3Uploader = s3ImageUploader;
        }

        public IImageCompressor ImageCompressor { get => imageComp; set => imageComp = value; }
        public IS3ImageUploader S3ImageUploader { get => s3Uploader; set => s3Uploader = value; }

        public async Task<ImageServiceResponse> CompressAndUploadImageAsync(Stream sourceImageStream, string fileKeyName = "")
        {
            var tmpFile = Path.GetTempFileName();
            var comp = await imageComp.CompressImage(sourceImageStream, tmpFile);
            if (!comp.ImageCompressionSucccess)
                throw new Exception("Image compression failed.");
            var uploaderResp = await s3Uploader.UploadAsync(File.OpenRead(tmpFile), key: fileKeyName);
            return SendResponse(uploaderResp, comp);
        }

        public async Task<ImageServiceResponse> CompressAndUploadImageAsync(string sourceFilePath, string fileKeyName = "")
        {
            return await CompressAndUploadImageAsync(File.OpenRead(sourceFilePath), fileKeyName);
        }

        public async Task<ImageServiceResponse> CompressWaterMarkAndUploadAsync(Stream sourceImageStream, Stream watermarkImage, string fileKeyName = "")
        {
            var tmpFile = Path.GetTempFileName();
            var watermarkSuccess = await this.imageComp.AddWaterMark(sourceImageStream, watermarkImage, tmpFile);
            return await CompressAndUploadImageAsync(File.OpenRead(tmpFile), fileKeyName);
        }

        public async Task<ImageServiceResponse> CompressWaterMarkAndUploadAsync(string sourceFilePath, Stream watermarkImage, string fileKeyName = "")
        {
            return await CompressWaterMarkAndUploadAsync(File.OpenRead(sourceFilePath), watermarkImage, fileKeyName);
        }

        private ImageServiceResponse SendResponse(PutObjectResponse putObjectResponse, ImageCompressorResponse imageCompressorResponse)
        {
            return new ImageServiceResponse
            {
                ImageCompressorResponse = imageCompressorResponse,
                PutObjectResponse = putObjectResponse,
            };
        }
    }
}
