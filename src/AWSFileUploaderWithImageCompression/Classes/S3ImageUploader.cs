using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace AWSFileUploaderWithImageCompression;

public sealed class S3ImageUploader : IS3ImageUploader
{
    private readonly IAmazonS3 s3Client;
    private readonly string bucketName;
    private readonly bool ownsClient;

    public S3ImageUploader(string accessKey, string secretKey, RegionEndpoint regionEndpoint, string bucketName)
        : this(CreateClient(accessKey, secretKey, regionEndpoint), bucketName, true)
    {
    }

    public S3ImageUploader(IAmazonS3 amazonS3Client, string bucketName)
        : this(amazonS3Client, bucketName, false)
    {
    }

    private S3ImageUploader(IAmazonS3 amazonS3Client, string bucketName, bool ownsClient)
    {
        s3Client = amazonS3Client ?? throw new ArgumentNullException(nameof(amazonS3Client));
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new ArgumentException("Bucket name must be provided.", nameof(bucketName));
        }

        this.bucketName = bucketName;
        this.ownsClient = ownsClient;
    }

    public async Task<PutObjectResponse> UploadAsync(Stream sourceImageStream, string bucketName = "", string key = "", S3StorageClass? storageClass = null)
    {
        ArgumentNullException.ThrowIfNull(sourceImageStream);

        var putReq = new PutObjectRequest
        {
            BucketName = string.IsNullOrWhiteSpace(bucketName) ? this.bucketName : bucketName,
            Key = string.IsNullOrWhiteSpace(key) ? Guid.NewGuid().ToString() : key,
            StorageClass = storageClass ?? S3StorageClass.Standard,
            AutoResetStreamPosition = true,
            InputStream = sourceImageStream,
        };

        return await s3Client.PutObjectAsync(putReq).ConfigureAwait(false);
    }

    public async Task<PutObjectResponse> UploadAsync(string sourceImagePath, string bucketName = "", string key = "", S3StorageClass? storageClass = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceImagePath);

        await using var sourceStream = File.OpenRead(sourceImagePath);
        return await UploadAsync(sourceStream, bucketName, key, storageClass).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (!ownsClient)
        {
            return;
        }

        switch (s3Client)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }

    private static IAmazonS3 CreateClient(string accessKey, string secretKey, RegionEndpoint regionEndpoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);
        ArgumentNullException.ThrowIfNull(regionEndpoint);

        var awsCred = new BasicAWSCredentials(accessKey, secretKey);
        return new AmazonS3Client(awsCred, regionEndpoint);
    }
}
