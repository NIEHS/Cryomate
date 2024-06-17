using System.Windows.Media.Imaging;
using SprayingSystem.IdsDriver;

namespace SprayingSystem.CameraModule
{
    public interface ICameraController
    {
        void Open();

        void HookHandlers(
            BackEnd.ImageReceivedEventHandler imageReceivedEvh,
            BackEnd.MessageBoxTriggerEventHandler messageBoxEvh = null,
            BackEnd.CountersUpdatedEventHandler countersEvh = null);

        bool Start();
        void Stop();
        void Close();

        void SetGain(double gain);
        BitmapImage TakePicture();
        void SavePicture(string filename);
        void SavePicture(string filename, BitmapImage image);
    }

}
