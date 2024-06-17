using System.Collections.Generic;
using System.Drawing;
using SprayingSystem.ImageProcess;

namespace SprayingSystem.CameraModule
{
    public class CameraRecording
    {
        public bool IsRecording;

        public int FrameCount
        {
            get
            {
                if (_bitmaps != null)
                    return _bitmaps.Count;
                return 0;
            }
        }

        private List<Bitmap> _bitmaps = null;

        public void Start()
        {
            _bitmaps = new List<Bitmap>();
            IsRecording = true;
        }

        public void Store(Bitmap bitmap)
        {
            if (_bitmaps == null)
                return;

            var storeBitmap = (Bitmap)bitmap.Clone();
            _bitmaps.Add(storeBitmap);
        }

        public void Stop()
        {
            IsRecording = false;
        }

        public void Save(string filename)
        {
            CreateVideoFile.FromBitmaps(_bitmaps, filename);
        }
    }
}
