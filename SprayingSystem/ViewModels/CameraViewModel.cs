using Microsoft.AspNetCore.Components;
using SprayingSystem.CameraModule;
using SprayingSystem.ImageProcess;
using SprayingSystem.Models;
using SprayingSystem.Utility;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SprayingSystem.ViewModels
{
    public class CameraViewModel : ViewModelBase
    {
        #region Fields

        private readonly InMemoryLogProvider _logProvider;

        private ICameraController _camera;
        private bool _saveNextImageRequested;
        private bool _takePictureRequested;
        private bool _isLiveMode;
        private bool _isConnected;
        private BitmapImage _cameraImage;
        private int _frameCount;
        private int _frameErrors;
        private CameraRecording _cameraRecording;

        private string _tempImageFolder = @"c:\temp\Venkata_IDS\";

        private UserOptions _userOptions;

        private ICommand _liveCmd;
        private ICommand _connectCmd;
        private ICommand _takePictureCmd;
        private ICommand _savePictureCmd;
        private ICommand _powerDownCmd;
        private ICommand _startRecordingCmd;
        private ICommand _stopRecordingCmd;
        private ICommand _saveVideoCmd;

        #endregion


        // TODO: camera - when we take a picture save the datetime stamp, display it.
        // Save the date and time overlay on the image?


        public CameraViewModel(UserOptions userOptions, InMemoryLogProvider logProvider)
        {
            _logProvider = logProvider ?? throw new ArgumentNullException(nameof(logProvider));
            _userOptions = userOptions;

            _tempImageFolder = _userOptions.Options.camera.temp_folder;

            _connectCmd = new RelayCommand(Connect, CanConnect);
            _liveCmd = new RelayCommand(Live, CanLive);
            _takePictureCmd = new RelayCommand(TakePicture, CanTakePicture);
            _savePictureCmd = new RelayCommand(SavePicture, CanSavePicture);
            _powerDownCmd = new RelayCommand(PowerDown, CanPowerDown);
            _startRecordingCmd = new RelayCommand(StartRecording, CanRecord);
            _stopRecordingCmd = new RelayCommand(StopRecording, CanRecord);
            _saveVideoCmd = new RelayCommand(SaveVideo, CanRecord);

            _cameraRecording = new CameraRecording();
        }

        public bool IsLiveMode
        {
            get => _isLiveMode;
            set
            {
                _isLiveMode = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCmd => _connectCmd;
        public ICommand LiveCmd => _liveCmd;
        public ICommand TakePictureCmd => _takePictureCmd;
        public ICommand SavePictureCmd => _savePictureCmd;
        public ICommand PowerDownCmd => _powerDownCmd;
        public ICommand StartRecordingCmd => _startRecordingCmd;
        public ICommand StopRecordingCmd => _stopRecordingCmd;
        public ICommand SaveVideoCmd => _saveVideoCmd;

        public void Disconnect()
        {
            _camera.Close();
        }

        public BitmapImage CameraImage
        {
            get { return _cameraImage; }
            set
            {
                _cameraImage = value;
                OnPropertyChanged();
            }
        }

        public int FrameCount
        {
            get { return _frameCount; }
            set
            {
                _frameCount = value;
                OnPropertyChanged();
            }
        }

        public int FrameErrors
        {
            get { return _frameErrors; }
            set
            {
                _frameErrors = value;
                OnPropertyChanged();
            }
        }

        #region Private

        #region Camera event handlers

        private void ImageReceivedHandler(object sender, Bitmap bitmap)
        {
            try
            {
                // and not convert the image to BitmapImage. Faster.
                if (_cameraRecording.IsRecording)
                {
                    _cameraRecording.Store(bitmap);
                    return;
                }

                // It is important to display the image first.
                // Otherwise we may not be able to TakePicture correctly.
                DisplayImage(bitmap);

                SaveImageIfRequested(bitmap);
            }
            catch (Exception e)
            {
                _logProvider.CreateLogger(nameof(AppViewModel)).LogError(e.Message);
            }
        }

        private void CountersUpdatedHandler(object sender, uint frameCounter, uint errorCounter)
        {
            try
            {
                ++FrameCount;
                FrameErrors = (int)errorCounter;
            }
            catch (Exception e)
            {
                _logProvider.CreateLogger(nameof(AppViewModel)).LogError(e.Message);
            }
        }

        private void MessageBoxHandler(object sender, String messageTitle, String messageText)
        {
            _logProvider.CreateLogger(nameof(AppViewModel)).LogInformation(messageTitle + ": " + messageText);
        }

        #endregion

        private void InitializeCameraController()
        {
            try
            {
                if (_camera != null)
                    _camera.Close();

                _camera = new CameraController();
                _camera.Open();
                _camera.HookHandlers(ImageReceivedHandler, MessageBoxHandler, CountersUpdatedHandler);

                IsLiveMode = true;
                _saveNextImageRequested = false;

                if (!_camera.Start())
                {
                    _camera = null;
                    IsConnected = false;
                }
                else
                    IsConnected = true;
            }
            catch (Exception e)
            {
                _logProvider.CreateLogger(nameof(AppViewModel)).LogError(e.Message);
                IsConnected = false;
            }
        }

        private void DisplayImage(Bitmap bitmap)
        {
            // We gather frames if in live mode or if we need to take a picture.
            if (IsLiveMode || _takePictureRequested)
                CameraImage = FormatConverter.ToBitmapImage(bitmap);
        }

        private void SaveImageBeingDisplayed()
        {
            if (CameraImage != null)
                SaveImageWithUniqueName(_tempImageFolder, CameraImage);
        }

        private void SaveImageIfRequested(Bitmap bitmap)
        {
            if (_saveNextImageRequested)
            {
                SaveImageWithUniqueName(_tempImageFolder, FormatConverter.ToBitmapImage(bitmap));

                _saveNextImageRequested = false;

                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    IsLiveMode = false;
                });
            }

            if (_takePictureRequested)
            {
                _takePictureRequested = false;

                // NOTE: Don't do this double updates on variables.
                // It is best to do it from the UI thread.
                //Dispatcher.CurrentDispatcher.Invoke(() =>
                //{
                //    IsLiveMode = false;
                //});
            }
        }

        private void SaveImageWithUniqueName(string folder, BitmapImage bitmapImage)
        {
            try
            {
                var fullname = FileUtil.CreateFilenameWithDateTime(
                    folder, "Img_", "jpg");

                FileUtil.CreateFolderIfNotExist(fullname);

                _camera.SavePicture(fullname, bitmapImage);

                _logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("Camera image saved");
            }
            catch (Exception e)
            {
                _logProvider.CreateLogger(nameof(AppViewModel)).LogError("Failed to save image: " + e.Message);
            }
        }

        private void StartLiveMode()
        {
            if (!IsLiveMode)
                IsLiveMode = true;
        }

        private bool CanConnect(object obj)
        {
            return !IsConnected;
        }

        private void Connect(object obj)
        {
            _logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("Camera status: connecting");
            InitializeCameraController();
            if (IsConnected)
                _logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("Camera status: connected");
        }

        private bool CanLive(object obj)
        {
            return IsConnected && !IsLiveMode;
        }

        private void Live(object obj)
        {
            IsLiveMode = true;
        }

        private bool CanTakePicture(object obj)
        {
            return IsConnected;
        }

        private void TakePicture(object obj)
        {
            IsLiveMode = false;
            _takePictureRequested = true;
        }

        private bool CanSavePicture(object obj)
        {
            return IsConnected;
        }

        private void SavePicture(object obj)
        {
            if (IsLiveMode)
                _saveNextImageRequested = true;
            else
                SaveImageBeingDisplayed();
        }

        private bool CanPowerDown(object obj)
        {
            return IsConnected;
        }

        private void PowerDown(object obj)
        {
            try
            {
                if (IsConnected)
                {
                    _camera?.Close();
                    IsConnected = false;
                }
            }
            catch (Exception e)
            {
                _logProvider.CreateLogger(nameof(AppViewModel)).LogError(e.Message);
            }
        }

        private bool CanRecord(object obj)
        {
            return IsConnected;
        }

        private void StartRecording(object obj)
        {
            _cameraRecording.Start();
            _logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("Started video recording");
        }

        private void StopRecording(object obj)
        {
            _cameraRecording.Stop();
            _logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("Stopped video recording");
        }

        private void SaveVideo(object obj)
        {
            _cameraRecording.Save(@"c:\temp\video.MP4");
            _logProvider.CreateLogger(nameof(AppViewModel)).LogInformation($"({_cameraRecording.FrameCount}frames): Saving video recording");
        }

        #endregion

    }
}