using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace SprayingSystem.ImageProcess
{
    public class FormatConverter
    {
        public static BitmapImage ToBitmapImage(Bitmap bmp)
        {
            var memory = new MemoryStream();

            bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

    }
}
