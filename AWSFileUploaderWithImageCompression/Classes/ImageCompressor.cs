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
                serviceConfiguration = (x) =>
                {
                    _ = new ImgCompressorConfiguration();
                };

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

                // image.Width and image.Height are uint in new Magick.NET
                uint currentWidth = image.Width;
                uint currentHeight = image.Height;

                // Ensure uint arithmetic
                uint targetWidth = currentWidth > 10u ? currentWidth - 10u : 0u;
                uint targetHeight = currentHeight * 25u / 100u;

                watermark.Resize(targetWidth, targetHeight); // MagickImage.Resize(uint, uint)
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
                using var img = new MagickImage(imageStream); // img.Height, img.Width are uint
                uint imgHeight = img.Height;
                uint imgWidth = img.Width;
                int originalHeight = (int)imgHeight; // Cast to int for response model
                int originalWidth = (int)imgWidth;   // Cast to int for response model

                if (imgHeight > (uint)serviceConfiguration.MaxHeight || imgWidth > (uint)serviceConfiguration.MaxWidth)
                {
                    var imgGeo = new MagickGeometry // MagickGeometry.Height/Width are now uint based on error
                    {
                        IgnoreAspectRatio = !serviceConfiguration.MaintainAspectRatio
                    };
                    // Assign uint to uint for MagickGeometry
                    if (imgHeight > (uint)serviceConfiguration.MaxHeight)
                        imgGeo.Height = (uint)serviceConfiguration.MaxHeight; // Cast int to uint
                    else if (imgWidth > (uint)serviceConfiguration.MaxWidth)
                        imgGeo.Width = (uint)serviceConfiguration.MaxWidth; // Cast int to uint
                    img.Resize(imgGeo); // Resize overload that takes MagickGeometry
                }
                else
                {
                    return new ImageCompressorResponse
                    {
                        ImageCompressionSucccess = false,
                        OriginalImageHeight = originalHeight, // Already int
                        OriginalImageWidth = originalWidth,   // Already int
                        OriginalSizeInBytes = imageStream.Length,
                    };
                }

                using var memStream = new MemoryStream();
                await img.WriteAsync(memStream);

                var optimizer = new ImageOptimizer();
                memStream.Position = 0;
                // Compare uint with int (promoted to uint)
                var comp = (imgHeight >= (uint)serviceConfiguration.MaxHeight || imgWidth >= (uint)serviceConfiguration.MaxWidth) && optimizer.LosslessCompress(memStream);
                await File.WriteAllBytesAsync(outputFilePath, memStream.ToArray());
                using var outputImage = new MagickImage(memStream); // outputImage.Height/Width are uint
                return new ImageCompressorResponse
                {
                    ImageCompressionSucccess = comp,
                    OriginalSizeInBytes = imageStream.Length,
                    AfterCompressionSizeInBytes = memStream.Length,
                    OutputFilePath = outputFilePath,
                    OutPutFileStream = memStream,
                    OriginalImageWidth = originalWidth,     // Already int
                    OriginalImageHeight = originalHeight,   // Already int
                    ResizedImageHeight = (int)outputImage.Height, // Cast uint to int
                    ResizedImageWidth = (int)outputImage.Width,   // Cast uint to int
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
