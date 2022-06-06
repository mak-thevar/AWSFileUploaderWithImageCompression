using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace AWSFileUploaderWithImageCompression
{
    public class S3ImageUploader : IS3ImageUploader
    {
        private readonly string accessKey;
        private readonly string secretKey;
        private readonly RegionEndpoint regionEndpoint;
        private readonly string _bucketName;
        private readonly BasicAWSCredentials awsCred;

        public S3ImageUploader(string accessKey, string secretKey, RegionEndpoint regionEndpoint, string bucketName)
        {
            this.accessKey = accessKey;
            this.secretKey = secretKey;
            this.regionEndpoint = regionEndpoint;
            this._bucketName = bucketName;
            this.awsCred = new BasicAWSCredentials(this.accessKey, this.secretKey);
        }

        public async Task<PutObjectResponse> UploadAsync(Stream sourceImageStream, string bucketName = "", string key = "", S3StorageClass? storageClass = null)
        {
            var s3Client = new AmazonS3Client(awsCred, regionEndpoint);
            var putReq = new PutObjectRequest();
            putReq.BucketName = bucketName;
            putReq.Key = string.IsNullOrEmpty(key) ? Guid.NewGuid().ToString() : key;
            putReq.StorageClass = storageClass ?? S3StorageClass.Standard;
            var putResp = await s3Client.PutObjectAsync(putReq);
            return putResp;
        }


        public async Task<PutObjectResponse> UploadAsync(string sourceImagePath, string bucketName = "", string key = "", S3StorageClass? storageClass = null)
        {
            return await UploadAsync(File.OpenRead(sourceImagePath), bucketName, key, storageClass);
        }


    }
}
