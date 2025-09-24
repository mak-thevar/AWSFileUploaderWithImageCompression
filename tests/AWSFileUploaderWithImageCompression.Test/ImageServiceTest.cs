using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Moq;
using NUnit.Framework;

namespace AWSFileUploaderWithImageCompression.Test;

[TestFixture]
public class ImageServiceTest
{
    private IImageService imgService = null!;

    [OneTimeSetUp]
    public void Setup()
    {
        var s3Uploader = new Mock<IS3ImageUploader>();
        s3Uploader.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), null))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });
        s3Uploader.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);

        imgService = new ImageService(s3Uploader.Object);
    }

    /// <summary>
    /// This will test the resize and compression of large image files.
    /// </summary>
    [Test, TestCaseSource(typeof(TestSourceProvider), nameof(TestSourceProvider.GetLargeImages))]
    public async Task CompressLargeImagesTestAsync(FileInfo originalFile)
    {
        var outputDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Compressed");
        Directory.CreateDirectory(outputDirectory);
        var outputFileName = Path.Combine(outputDirectory, Guid.NewGuid() + originalFile.Extension);

        var result = await imgService.ImageCompressor.CompressImage(originalFile.FullName, outputFileName);
        var serializedResult = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

        if (result.ImageCompressionSucccess || result.AfterCompressionSizeInBytes < result.OriginalSizeInBytes)
        {
            Assert.Pass($"Successfully compressed the image\n-------------------------------------\nFileName: {originalFile.Name}\n{serializedResult}");
        }

        Assert.Fail($"Compression Failed!\n-------------------------------------\nFileName: {originalFile.Name}\n{serializedResult}");
    }

    /// <summary>
    /// This will test that no compression should be performed if the images are already smaller in size.
    /// </summary>
    [Test, TestCaseSource(typeof(TestSourceProvider), nameof(TestSourceProvider.GetSmallImages))]
    public async Task DontCompressSmallImageAsync(FileInfo originalFile)
    {
        var outputDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Compressed");
        Directory.CreateDirectory(outputDirectory);
        var outputFileName = Path.Combine(outputDirectory, Guid.NewGuid() + originalFile.Extension);

        var result = await imgService.ImageCompressor.CompressImage(originalFile.FullName, outputFileName);
        _ = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

        Assert.IsFalse(result.ImageCompressionSucccess);
    }

    /// <summary>
    /// This will test the if the watermark to the image has been applied successfully.
    /// </summary>
    [Test]
    public async Task WaterMarkTest()
    {
        await using var watermarkImage = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Original", "SmallImages", "spiderman.png"));
        var outputDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Watermark");
        Directory.CreateDirectory(outputDirectory);
        await using var sourceFile = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Original", "LargeImages", "2.jpg"));
        var outputFile = Path.Combine(outputDirectory, $"{Guid.NewGuid()}.jpg");

        imgService.ImageCompressor.UpdateImageServiceConfiguration(options =>
        {
            options.WaterMarkTransperency = 5;
        });

        var success = await imgService.ImageCompressor.AddWaterMark(sourceFile, watermarkImage, outputFile);

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
        await using var sourceFile = File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Original", "LargeImages", fileName));
        var resp = await imgService.CompressAndUploadImageAsync(sourceFile);

        Assert.IsNotNull(resp);
        Assert.That(resp.PutObjectResponse?.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [OneTimeTearDown]
    public void ClearImages()
    {
        var compressedPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Compressed");
        if (Directory.Exists(compressedPath))
        {
            Directory.Delete(compressedPath, true);
        }

        var watermarkPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Watermark");
        if (Directory.Exists(watermarkPath))
        {
            Directory.Delete(watermarkPath, true);
        }
    }
}
