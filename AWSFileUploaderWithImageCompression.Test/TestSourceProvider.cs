using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSFileUploaderWithImageCompression.Test
{
    public static class TestSourceProvider
    {
        public static FileInfo[] GetLargeImages()
        {
            return new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Assets", "Original", "LargeImages")).GetFiles();
        }

        public static FileInfo[] GetSmallImages()
        {
            return new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Assets", "Original", "SmallImages")).GetFiles();
        }
    }
}
