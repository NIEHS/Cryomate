using RCAPINet;
using SprayingSystem.ViewModels;
using System.Windows;

namespace SprayingSystem.RobotFeatures
{
    public class SimulatorPresenter
    {
        private Spel _spel;
        private readonly ILogger<AppViewModel> _logger;

        public SimulatorPresenter(Spel spel, ILogger<AppViewModel> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _spel = spel;
        }

        public void Initialize(Spel spel)
        {
            _spel = spel;
        }

        public void Show()
        {
            try
            {
                _spel.ShowWindow(RCAPINet.SpelWindows.Simulator);
            }
            catch (SpelException ex)
            {
                _logger.LogError(ex.Message + "Failed to show Simulator");
            }
        }
    }
}

