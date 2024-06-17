using System.Windows;

namespace SprayingSystem.SprayingSystemConfigViewer
{
    /// <summary>
    /// Interaction logic for JsonTreeView.xaml
    /// </summary>
    public partial class JsonTreeView : Window
    {
        private JsonTreeViewModel _viewModel;
        private readonly InMemoryLogProvider _logProvider;

        public JsonTreeView(string filename, InMemoryLogProvider logProvider)
        {
            //InitializeComponent();
            _logProvider = logProvider ?? throw new ArgumentNullException(nameof(logProvider));

            _viewModel = new JsonTreeViewModel(filename, _logProvider);
            DataContext = _viewModel;
            _logProvider = logProvider;
        }

        public JsonTreeViewModel.EditStatus Edits
        {
            get { return _viewModel.Edits; }
        }
    }
}
