using System.IO;
using NUnit.Framework;

namespace AWSFileUploaderWithImageCompression.Test;

public static class TestSourceProvider
{
    public static FileInfo[] GetLargeImages()
    {
        var directory = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Original", "LargeImages"));
        return directory.GetFiles();
    }

    public static FileInfo[] GetSmallImages()
    {
        var directory = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Original", "SmallImages"));
        return directory.GetFiles();
    }
}
