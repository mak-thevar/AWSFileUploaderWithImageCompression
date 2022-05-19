# AWSFileUploaderWithImageCompression
A library for compressing, adding watermark and uploading the file into aws s3.

This library uses [Magic.NET](https://github.com/dlemstra/Magick.NET) for compressing and adding watermark to the image.

## Features
- Compress the image in a simplied way.
- Seperation of logic for uploader, compression and adding watermark.
- Save s3 bucket space by compressing the image before uploading it.

## Installation
- With package manager :
```Install-Package AWSFileUploaderWithImageCompression```
- With dotnet cli :
```dotnet add package AWSFileUploaderWithImageCompression```
- Other options and version history :
 https://www.nuget.org/packages/AWSFileUploaderWithImageCompression

## Usage
#### ⚙️Using the image service class
```csharp
  //Initialize the s3imageuploader with respective access_key, secret_key and the bucketname
  IS3ImageUploader s3ImageUploader = new S3ImageUploader("ACCESS_KEY", "SECRET_KEY", RegionEndpoint.APSouth1, "BUCKET_NAME");
  IImageService imageService = new ImageService(s3ImageUploader);
  PutObjectResponse objectResponse =  await imageService.CompressAndUploadImageAsync(System.IO.File.OpenRead("C:/myfolder/mypic.jpg"), "newname.jpg" /* Optional to provide a new filename by default it uses Guid for the uploaded filename.*/); 
```
#### ⚙️Using the individual classes
```csharp
ImgCompressorConfiguration imgCompressorConfiguration = new ImgCompressorConfiguration
{
    MaintainAspectRatio = true,
    MaxHeight = 600, //Default is 1200
    MaxWidth = 800, //Default is 1600
    WaterMarkPosition = ImageMagick.Gravity.Northeast, //Default is Southeast (bottom right)
    WaterMarkTransperency = 5 //The watermark transperency ranges from 0 to 10, 0 as 0% and 10 as 100% transperency.
};
IImageCompressor compressor = new ImageCompressor(imgCompressorConfiguration /* Optional */);
//For adding watermark to the image
bool watermarkSucess = await compressor.AddWaterMark("C:/myfolder/mypic.jpg", "C:/myfolder/icons/watermark.png",
    "C:/myfolder/output/myimage.jpg");

//For compressing the image (Note: The compression will happen when the source image exceeds the MaxHeight or MaxWidth configuration.
ImageCompressorResponse compressionSuccess = await compressor.CompressImage("C:/myfolder/mypic.jpg", "C:/myfolder/output/myimage.jpg");
/* The response will have the following data :
 * 1. ImageCompressionSucccess
 * 2. OriginalSizeInBytes
 * 3. OriginalImageWidth
 * 4. OriginalImageHeight
 * 5. AfterCompressionSizeInBytes
 * 6. ResizedImageHeight
 * 7. ResizedImageWidth
 * 8. OutputFilePath
 * 9. OutPutFileStream (If output was provided as a stream)
 */
  ```

## Contributing
Contributions are what make the open source community such an amazing place to be learn, inspire, and create. Any contributions you make are greatly appreciated.
Feel free to request for any changes or any additional features.

## License
Distributed under the MIT License. See [LICENSE](https://github.com/mak-thevar/AWSFileUploaderWithImageCompression/blob/main/LICENSE) for more information.

## Contact
Name: [Muthukumar Thevar](https://www.linkedin.com/in/mak11/)

Email: mak.thevar@outlook.com

Project Link: https://github.com/mak-thevar/AWSFileUploaderWithImageCompression
