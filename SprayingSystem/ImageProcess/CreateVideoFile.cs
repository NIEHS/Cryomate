using System.Collections.Generic;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using FFMediaToolkit;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace SprayingSystem.ImageProcess
{
    /// <summary>
    /// Requires: FFMediaToolkit, a NuGet package for FFMpeg
    /// https://github.com/radek-k/FFMediaToolkit
    /// </summary>
    public class CreateVideoFile
    {
        private static bool IsLoaded = false;

        public void Blah()
        {
            FFmpegLoader.FFmpegPath = @"C:\FFmpeg\";

            var settings = new VideoEncoderSettings(
                width: 960, height: 544, framerate: 50, codec: VideoCodec.H264);

            settings.EncoderPreset = EncoderPreset.Fast;
            settings.CRF = 17;

            var file = MediaBuilder.CreateContainer(@"C:\out-video\out.mp4").WithVideo(settings).Create();
            var files = Directory.GetFiles(@"C:\Input\");

            foreach (var inputFile in files)
            {
                var binInputFile = File.ReadAllBytes(inputFile);
                var memInput = new MemoryStream(binInputFile);
                var bitmap = Bitmap.FromStream(memInput) as Bitmap;
                var rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size);
                var bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bitmap.Size);
                file.Video.AddFrame(bitmapData); // Encode the frame
                bitmap.UnlockBits(bitLock);
            }

            file.Dispose();
        }

        public static void FromBitmaps(List<Bitmap> bitmaps, string outputFilename)
        {
            // We need to download FFMpeg to a known folder like this.
            if (IsLoaded == false)
                FFmpegLoader.FFmpegPath = @"C:\FFmpeg\";
            IsLoaded = true;

            if (bitmaps == null || bitmaps.Count < 1)
                return;

            var size = GetBitmapImageInfo(bitmaps[0]);

            //var settings = new VideoEncoderSettings(
            //    width: size.Width, height: size.Height, 
            //    framerate: 50, 
            //    codec: VideoCodec.H264);

            // VideoCodec.AV1 - froze, did not finish encoding?

            var settings = new VideoEncoderSettings(
                width: size.Width, height: size.Height,
                framerate: 10,
                codec: VideoCodec.H264);

            settings.EncoderPreset = EncoderPreset.Fast;
            settings.CRF = 17;

            var file = MediaBuilder
                .CreateContainer(outputFilename)
                .WithVideo(settings)
                .Create();

            foreach (var bitmap in bitmaps)
            {
                var rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size);
                var bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bitmap.Size);

                // Encode the frame
                file.Video.AddFrame(bitmapData);
                bitmap.UnlockBits(bitLock);
            }

            file.Dispose();
        }

        private static Size GetBitmapImageInfo(Bitmap bitmap)
        {
            return bitmap.Size;
        }
    }
}
