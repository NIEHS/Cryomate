using System.Windows.Media.Imaging;
using SprayingSystem.IdsDriver;
using SprayingSystem.ImageProcess;

namespace SprayingSystem.CameraModule
{
    public class CameraController : ICameraController
    {
        private BackEnd backEnd;
        private bool _hasError = true;

        public void Open()
        {
            backEnd = new BackEnd();
        }

        public void HookHandlers(
            BackEnd.ImageReceivedEventHandler imageReceivedEvh,
            BackEnd.MessageBoxTriggerEventHandler messageBoxEvh = null,
            BackEnd.CountersUpdatedEventHandler countersEvh = null)
        {
            backEnd.ImageReceived += imageReceivedEvh;
            backEnd.CountersUpdated += countersEvh;
            backEnd.MessageBoxTrigger += messageBoxEvh;
        }

        public bool Start()
        {
            if (backEnd.Start())
                _hasError = false;
            else
                _hasError = true;
            return !_hasError;
        }

        // TODO: camera - What is the difference between Stop and Close? Keep one?

        public void Stop()
        {
            backEnd.Stop();
        }

        public void Close()
        {
            backEnd.Stop();
        }

        public void SetGain(double gain)
        {
            // TODO: camera - How do we camera gain?
        }

        /// <summary>
        /// If it is in live mode, it will save current a frame.
        /// </summary>
        public BitmapImage TakePicture()
        {
            return null;
        }

        /// <summary>
        /// Will save image from live feed.
        /// </summary>
        public void SavePicture(string filename)
        {

        }

        /// <summary>
        /// Saves the image in JPEG format.
        /// </summary>
        public void SavePicture(string filename, BitmapImage image)
        {
            SaveImage.SaveBitmapImageAsJpeg(image, filename);
        }
    }
}