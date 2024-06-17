using System.IO;
using System.Windows.Media.Imaging;

namespace SprayingSystem.ImageProcess
{
    public class SaveImage
    {
        public static void SaveBitmapImageAsJpeg(BitmapImage bitmapImage, string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                var encoder = new JpegBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(stream);
                stream.Close();
            }
        }
    }
}
