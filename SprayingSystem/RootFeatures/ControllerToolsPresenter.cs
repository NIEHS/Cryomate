using RCAPINet;
using SprayingSystem.ViewModels;
using System.Windows;

namespace SprayingSystem.RobotFeatures
{
    public class ControllerToolsPresenter
    {
        private Spel _spel;
        private readonly InMemoryLogProvider _logProvider;

        public ControllerToolsPresenter(Spel spel, InMemoryLogProvider logProvider)
        {
            _logProvider = logProvider ?? throw new ArgumentNullException(nameof(logProvider));
            _spel = spel;
        }

        public void Initialize(Spel spel)
        {
            _spel = spel;
        }

        public bool CanShow(object obj)
        {
            return _spel != null;
        }

        public void Show(object obj)
        {
            try
            {
                _spel.RunDialog(SpelDialogs.ControllerTools);
            }
            catch (SpelException ex)
            {
                _logProvider.CreateLogger(nameof(AppViewModel)).LogError(ex.Message + " Error showing controller tools dialog");
            }
        }
    }
}
