using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace AWSFileUploaderWithImageCompression;

public interface IS3ImageUploader : IAsyncDisposable
{
    Task<PutObjectResponse> UploadAsync(Stream sourceImageStream, string bucketName = "", string key = "", S3StorageClass? storageClass = null);
    Task<PutObjectResponse> UploadAsync(string sourceImagePath, string bucketName = "", string key = "", S3StorageClass? storageClass = null);
}
