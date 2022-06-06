using AWSFileUploaderWithImageCompression.Models;
using ImageMagick;

namespace AWSFileUploaderWithImageCompression
{
    public class ImageCompressor : IImageCompressor
    {
        private ImgCompressorConfiguration serviceConfiguration;

        public ImageCompressor(ImgCompressorConfiguration? serviceConfiguration = null)
        {
            if (serviceConfiguration == null)
                serviceConfiguration = new ImgCompressorConfiguration();

            this.serviceConfiguration = serviceConfiguration;
        }

        public ImageCompressor(Action<ImgCompressorConfiguration>? serviceConfiguration = null)
        {
            if (serviceConfiguration == null)
                serviceConfiguration = (x) => new ImgCompressorConfiguration();

            var imgCompressorConfig = new ImgCompressorConfiguration();
            serviceConfiguration.Invoke(imgCompressorConfig);
            this.serviceConfiguration = imgCompressorConfig;
        }

        public void UpdateImageServiceConfiguration(ImgCompressorConfiguration serviceConfiguration)
        {
            this.serviceConfiguration = serviceConfiguration;
        }

        public void UpdateImageServiceConfiguration(Action<ImgCompressorConfiguration> serviceConfiguration)
        {
            var imgCompressorConfig = new ImgCompressorConfiguration();
            serviceConfiguration.Invoke(imgCompressorConfig);
            this.serviceConfiguration = imgCompressorConfig;
        }


        public async Task<bool> AddWaterMark(Stream sourceImageStream, Stream waterMarkImageStream, string outputFilePath)
        {
            try
            {
                using var watermark = new MagickImage(waterMarkImageStream);
                using var image = new MagickImage(sourceImageStream);

                watermark.Resize(image.Width - 10, image.Height * 25 / 100);
                watermark.Evaluate(Channels.Alpha, EvaluateOperator.Divide,serviceConfiguration.WaterMarkTransperency);

                image.Composite(watermark, serviceConfiguration.WaterMarkPosition, CompositeOperator.Over);

                await image.WriteAsync(outputFilePath);
                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<ImageCompressorResponse> CompressImage(Stream imageStream, string outputFilePath)
        {
            try
            {
                using var img = new MagickImage(imageStream);
                var originalHeight = img.Height;
                var originalWidth = img.Width;
                if (img.Height > serviceConfiguration.MaxHeight || img.Width > serviceConfiguration.MaxWidth)
                {
                    var imgGeo = new MagickGeometry();
                    imgGeo.IgnoreAspectRatio = !serviceConfiguration.MaintainAspectRatio;
                    if (img.Height > serviceConfiguration.MaxHeight)
                        imgGeo.Height = serviceConfiguration.MaxHeight;
                    else if (img.Width > serviceConfiguration.MaxWidth)
                        imgGeo.Width = serviceConfiguration.MaxWidth;
                    img.Resize(imgGeo);
                }
                else
                {
                    return new ImageCompressorResponse
                    {
                        ImageCompressionSucccess = false,
                        OriginalImageHeight = originalHeight,
                        OriginalImageWidth = originalWidth,
                        OriginalSizeInBytes = imageStream.Length,
                    };
                }

                using var memStream = new MemoryStream();
                await img.WriteAsync(memStream);

                var optimizer = new ImageOptimizer();
                memStream.Position = 0;
                var comp = (img.Height > serviceConfiguration.MaxHeight || img.Width > serviceConfiguration.MaxWidth) ? optimizer.LosslessCompress(memStream) : false;
                await File.WriteAllBytesAsync(outputFilePath, memStream.ToArray());
                using var outputImage = new MagickImage(memStream);
                return new ImageCompressorResponse
                {
                    ImageCompressionSucccess = comp,
                    OriginalSizeInBytes = imageStream.Length,
                    AfterCompressionSizeInBytes = memStream.Length,
                    OutputFilePath = outputFilePath,
                    OutPutFileStream = memStream,
                    OriginalImageWidth = originalWidth,
                    OriginalImageHeight = originalHeight,
                    ResizedImageHeight = outputImage.Height,
                    ResizedImageWidth = outputImage.Width,
                };
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<bool> AddWaterMark(string sourceImagePath, string watermarkImagePath, string outputFilePath)
        {
            return await AddWaterMark(File.OpenRead(sourceImagePath), File.OpenRead(watermarkImagePath), outputFilePath);
        }

        public async Task<ImageCompressorResponse> CompressImage(string sourceImagePath, string outputFilePath)
        {
            return await CompressImage(File.OpenRead(sourceImagePath), outputFilePath);
        }

    }
}
