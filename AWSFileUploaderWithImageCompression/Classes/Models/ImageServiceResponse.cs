using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSFileUploaderWithImageCompression.Classes.Models
{
    public class ImageServiceResponse
    {
        public PutObjectResponse PutObjectResponse { get; set; }
        public ImageCompressorResponse ImageCompressorResponse { get; set; }
    }
}
