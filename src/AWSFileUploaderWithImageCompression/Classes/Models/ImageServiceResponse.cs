using Amazon.S3.Model;

namespace AWSFileUploaderWithImageCompression.Models;

public class ImageServiceResponse
{
    public PutObjectResponse? PutObjectResponse { get; set; }
    public ImageCompressorResponse? ImageCompressorResponse { get; set; }
}
