using ImageMagick;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSFileUploaderWithImageCompression.Classes.Models
{
    public class ImgCompressorConfiguration
    {
        public int MaxWidth { get; set; } = 1600;
        public int MaxHeight { get; set; } = 1200;
        public bool MaintainAspectRatio { get; set; } = true;

        /// <summary>
        /// The direction to set the watermark
        /// </summary>
        public Gravity WaterMarkPosition { get; set; } = Gravity.Southeast;
        /// <summary>
        /// From 0 to 10, 10 = 100% transperency and 0 = 0% transperency
        /// </summary>
        public double WaterMarkTransperency { get; set; } = 3.5;
    }
}
