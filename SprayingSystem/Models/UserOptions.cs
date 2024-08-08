using static System.Net.Mime.MediaTypeNames;
using System.Windows;
using Microsoft.Extensions.Logging;
using Windows.Services.Maps;
using SprayingSystem.ViewModels;

namespace SprayingSystem.Models
{
    public class UserOptions
    {
        public SprayingSystemConfig Options;

        public void LoadSprayingSystemConfiguration(InMemoryLogProvider logProvider)
        {
            Options = SprayingSystemConfig.LoadSettings(logProvider);
            if (Options == null)
            {
                logProvider.CreateLogger(nameof(AppViewModel)).LogError("SprayingSystemConfig.json file not found. It is required for user options.");
                // Application.Current.Shutdown();
            }
            else
            {
                logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("SprayingSystemConfig.json loaded successfully.");
            }

            SetUserOptionsCamera();
            // SetUserOptionsRpi();
            SetUserOptionsRobot();
        }

        public void ReloadSprayingSystemConfiguration(InMemoryLogProvider logProvider)
        {
            var tempOptions = SprayingSystemConfig.LoadSettings(logProvider);
            if (tempOptions == null)
            {
                logProvider.CreateLogger(nameof(AppViewModel)).LogError("SprayingSystemConfig.json file not found. It is required for user options.");
                // Application.Current.Shutdown();
            }

            tempOptions.CopyTo(Options);
        }

        public void SaveSprayingSystemConfiguration()
        {
            Options.Save(SprayingSystemConfig.FileName);
        }

        private void SetUserOptionsCamera()
        {
            // TODO: User Options - save / load from file? _tempImageFolder = Options.camera.temp_folder;
        }

        /*
        private void SetUserOptionsRpi()
        {
            RpiEndPoint.IpAndPort =
                $"http://{Options.rpi.ip_address}:{Options.rpi.port}/";

            RpiEndPoint.MessageTimeoutSecs = Options.rpi.message_timeout_sec;
        }
        */

        private void SetUserOptionsRobot()
        {
            // TODO: user options - _robotConfig.RobotConfigurationNumber = Options.robot.id;
            // TODO: user options _robotConfig.RobotProject = Options.robot.project;

            // TODO: user options - What do we do with robot point number definitions? Fixed for now.
        }
    }

}
