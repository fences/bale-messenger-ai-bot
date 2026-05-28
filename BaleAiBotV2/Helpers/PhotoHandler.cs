using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BaleAiBotV2.Helpers
{
    public static class ImageCompressor
    {

        public static byte[] CompressToMaxSize(byte[]? imageBytes, int maxSizeBytes = 30720)
        {
            if (imageBytes == null || imageBytes.Length <= maxSizeBytes)
                return imageBytes; 

            using var ms = new MemoryStream(imageBytes);
            using var originalImage = Image.FromStream(ms);

            using var resizedImage = ResizeIfNeeded(originalImage, 800, 800);

            return CompressJpegToTargetSize(resizedImage, maxSizeBytes);
        }

        private static Image ResizeIfNeeded(Image image, int maxWidth, int maxHeight)
        {
            if (image.Width <= maxWidth && image.Height <= maxHeight)
                return (Image)image.Clone(); 

            double ratio = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            var bitmap = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return bitmap;
        }

        private static byte[] CompressJpegToTargetSize(Image image, int targetSize)
        {
            var encoderParameters = new EncoderParameters(1);
            var jpegCodec = GetJpegCodecInfo();

            int low = 1, high = 100, bestQuality = 100;
            byte[]? bestBytes = null;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)mid);

                using var ms = new MemoryStream();
                image.Save(ms, jpegCodec, encoderParameters);
                byte[] compressed = ms.ToArray();

                if (compressed.Length <= targetSize)
                {
                    bestQuality = mid;
                    bestBytes = compressed;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            if (bestBytes == null)
            {
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 1L);
                using var msFallback = new MemoryStream();
                image.Save(msFallback, jpegCodec, encoderParameters);
                bestBytes = msFallback.ToArray();
            }

            return bestBytes;
        }

        private static ImageCodecInfo GetJpegCodecInfo()
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.MimeType == "image/jpeg")
                    return codec;
            }
            throw new Exception("JPEG codec not found");
        }
    }
}