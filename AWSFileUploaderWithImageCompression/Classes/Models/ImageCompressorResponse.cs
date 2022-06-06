using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AWSFileUploaderWithImageCompression.Models
{
    public class ImageCompressorResponse
    {
        /// <summary>
        /// Whether the lossless compression was successfull if false then the image has been just resized (If the image had exceeded the maxWidth | maxHeight).
        /// </summary>
        public bool ImageCompressionSucccess { get; set; }
        public long OriginalSizeInBytes { get; set; }
        public int OriginalImageWidth { get; set; }
        public int OriginalImageHeight { get; set; }
        public long AfterCompressionSizeInBytes { get; set; }

        public int ResizedImageHeight { get; set; }
        public int ResizedImageWidth { get; set; }

        public string OutputFilePath { get; set; } = string.Empty;

        [JsonIgnore]
        public Stream? OutPutFileStream { get; set; } = null;
    }
}
