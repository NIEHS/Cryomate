using SprayingSystem.Models;
using SprayingSystem.Utility;
using SprayingSystem.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

namespace SprayingSystem.SprayingSystemConfigViewer
{
    public class JsonTreeViewModel : INotifyPropertyChanged
    {
        InMemoryLogProvider _logProvider;
        private ModelWrapper _model;
        private ICommand _saveCommand;
        private string _filename = @"JsonViewer\MyData.json";

        public JsonTreeViewModel(InMemoryLogProvider logProvider)
        {
            _logProvider = logProvider ?? throw new ArgumentNullException(nameof(logProvider));
            Initialize(_filename, _logProvider);
        }

        public JsonTreeViewModel(string settingsFilename, InMemoryLogProvider logProvider)
        {
            _logProvider = logProvider ?? throw new ArgumentNullException(nameof(logProvider));
            _filename = settingsFilename;
            Initialize(_filename, _logProvider);
        }

        public EditStatus Edits = new EditStatus();

        private void Initialize(string filename, InMemoryLogProvider logProvider)
        {
            _logProvider = logProvider ?? throw new ArgumentNullException(nameof(logProvider));
            var data = SprayingSystemConfig.LoadSettings(filename, _logProvider);

            _model = new ModelWrapper(data);

            _saveCommand = new RelayCommand(Save, obj => true);
        }

        private void Save(object obj)
        {
            /*
            var result = MessageBox.Show("Saving will override settings in your startup configuration. Are you sure to continue?",
                "Save Spraying Settings to file",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;
            */

            Model.SaveAsSprayingSystemConfig(_filename);

            Edits.FileSaved = true;
            Edits.CameraChanged = _model.IsCameraChanged;
            Edits.RpiChanged = _model.IsRpiChanged;

        }

        public ModelWrapper Model
        {
            get { return _model; }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand; }
            set
            {
                _saveCommand = value;
                OnPropertyChanged();
            }
        }

        #region Implements INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion


        public class EditStatus
        {
            public bool FileSaved = false;
            public bool CameraChanged = false;
            public bool RpiChanged = false;
            public bool RobotEssentialsChanged = false;
            public bool RobotMotionsChanged = false;
        }
    }
}
