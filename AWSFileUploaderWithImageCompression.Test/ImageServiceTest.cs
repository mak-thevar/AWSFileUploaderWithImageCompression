using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using Moq;
using Amazon.S3.Model;

namespace AWSFileUploaderWithImageCompression.Test
{
    public class ImageServiceTest
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private IImageService imgService;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        [OneTimeSetUp]
        public void Setup()
        {
            //Mocking s3Uploader
            var s3Uploader = new Mock<IS3ImageUploader>();
            s3Uploader.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), null)).Returns(Task.FromResult(new PutObjectResponse() { HttpStatusCode = System.Net.HttpStatusCode.OK }));

            imgService = new ImageService(s3Uploader.Object);
        }


        /// <summary>
        /// This will test the resize and compression of large image files.
        /// </summary>
        [Test, TestCaseSource(typeof(TestSourceProvider), nameof(TestSourceProvider.GetLargeImages))]
        public async Task CompressLargeImagesTestAsync(FileInfo originalFile)
        {
            //Arrange
            var outputDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Assets", "Compressed")).FullName;
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            var outputFileName = Path.Combine(outputDirectory, Guid.NewGuid().ToString() + originalFile.Extension);


            //Act
            var result = await imgService.ImageCompressor.CompressImage(originalFile.FullName, outputFileName);
            var serializedResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

            //Assert
            if (result.ImageCompressionSucccess || (result.AfterCompressionSizeInBytes < result.OriginalSizeInBytes))
                Assert.Pass($"Successfully compressed the image\n-------------------------------------\nFileName: {originalFile.Name}\n{serializedResult}");
            else
                Assert.Fail($"Compression Failed!\n-------------------------------------\nFileName: {originalFile.Name}\n{serializedResult}");

        }

        /// <summary>
        /// This will test that no compression should be performed if the images are already smaller in size.
        /// </summary>
        [Test, TestCaseSource(typeof(TestSourceProvider), nameof(TestSourceProvider.GetSmallImages))]
        public async Task DontCompressSmallImageAsync(FileInfo originalFile)
        {
            //Arrange
            var outputDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Assets", "Compressed")).FullName;
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            var outputFileName = Path.Combine(outputDirectory, Guid.NewGuid().ToString() + originalFile.Extension);


            //Act
            var result = await imgService.ImageCompressor.CompressImage(originalFile.FullName, outputFileName);
            _ = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

            //Assert
            Assert.IsTrue(result.ImageCompressionSucccess == false);
        }

        /// <summary>
        /// This will test the if the watermark to the image has been applied successfully.
        /// </summary>
        [Test]
        public async Task WaterMarkTest()
        {
            //Arrange
            var watermarkImage = File.OpenRead(Path.Combine(Environment.CurrentDirectory, "Assets", "Original", "SmallImages", "spiderman.png"));
            var outputDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Assets", "Watermark")).FullName;
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            var sourceFile = File.OpenRead(Path.Combine(Environment.CurrentDirectory, "Assets", "Original", "LargeImages", "2.jpg"));
            var outputFile = $"{Path.Combine(outputDirectory)}{Guid.NewGuid()}.jpg";


            var s3Uploader = new Mock<IS3ImageUploader>();
            s3Uploader.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), null)).Returns(Task.FromResult(new PutObjectResponse() { HttpStatusCode = System.Net.HttpStatusCode.OK }));


            imgService.ImageCompressor.UpdateImageServiceConfiguration(options =>
            {
                options.WaterMarkTransperency = 5; //50% transparency
            });

            //Act
            var success = await imgService.ImageCompressor.AddWaterMark(sourceFile, watermarkImage, outputFile);

            //Assert
            Assert.IsTrue(success);
        }



        /// <summary>
        /// This will test the Compress and Upload
        /// </summary>
        [TestCase("2.jpg")]
        [TestCase("3.jpg")]
        [TestCase("4.jpg")]
        public async Task CompressAndUploadTest(string fileName)
        {
            var sourceFile = File.OpenRead(Path.Combine(Environment.CurrentDirectory, "Assets", "Original", "LargeImages", fileName));
            var resp = await imgService.CompressAndUploadImageAsync(sourceFile);

            Assert.IsNotNull(resp);
            Assert.IsTrue(resp.PutObjectResponse?.HttpStatusCode == System.Net.HttpStatusCode.OK);
        }


        [OneTimeTearDown]
        public void ClearImages()
        {
            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Assets", "Compressed")))
                Directory.Delete(Path.Combine(Environment.CurrentDirectory, "Assets", "Compressed"), true);
            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Assets", "Watermark")))
                Directory.Delete(Path.Combine(Environment.CurrentDirectory, "Assets", "Watermark"), true);
        }
    }


}