using RCAPINet;
using SprayingSystem.ViewModels;
using System.Windows;

namespace SprayingSystem.RobotFeatures
{
    public class PointTeachingPresenter
    {
        private Spel _spel;
        private readonly InMemoryLogProvider _logProvider;

        public PointTeachingPresenter(Spel spel, InMemoryLogProvider logProvider)
        {
            _spel = spel;
            _logProvider = logProvider ?? throw new ArgumentNullException(nameof(logProvider));
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
                _spel.TeachPoint("robot1.pts", 1, "Teach Spray Position");
            }
            catch (SpelException ex)
            {
                _logProvider.CreateLogger(nameof(AppViewModel)).LogError(ex.Message + "Failed to show Point Teaching");
            }
        }
    }
}
