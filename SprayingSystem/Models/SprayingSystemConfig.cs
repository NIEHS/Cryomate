using SprayingSystem.ViewModels;
using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace SprayingSystem.Models
{
    public static class SprayingSystemConfigGroups
    {
        public static string Camera = "camera";
        public static string RPI = "rpi";
        public static string Robot = "robot";
        public static string BioJetProcess = "biojet_process";
    }

    public class CameraConfig
    {
        public string temp_folder { get; set; }
    }

    public class RpiConfig
    {
        public string ip_address { get; set; }
        public int port { get; set; }
        public int message_timeout_sec { get; set; }
        public string video_folder { get; set; }
        public int video_startup_delay_msec { get; set; }
        public int video_saving_delay_msec { get; set; }
    }

    public class RobotLocation
    {
        public string name { get; set; }
        public int point { get; set; }
        public int speed { get; set; }
        public string motiontype { get; set; }
    }

    public class GridStoreJumpParameters
    {
        public float zLimit { get; set; }
        public float zOffset { get; set; }
        public int archNumber { get; set; }
        public bool CP { get; set; }

        // ":Z(-20)"  is the same as setting zOffset=-20
        public string command { get; set; }
    }

    public class DefaultMotionParams
    {
        public int speed { get; set; }
        public int accel { get; set; }
        public int decel { get; set; }
    }

    public class RobotConfig
    {
        public int id { get; set; }
        public string project { get; set; }
        public bool simulation { get; set; }

        public DefaultMotionParams defaultMotionParams { get; set; }

        public RobotLocation[] locations { get; set; }

        public GridStoreJumpParameters gridStoreJumpParameters { get; set; }

        public void CopyTo(RobotConfig copyTo)
        {

        }
    }

    public class BioJetProcessConfig
    {
        public int prep_delay { get; set; } = 1500;
        public int spray_time { get; set; } = 0;
        public int blot_time { get; set; } = 0;
        public int clean_time { get; set; } = 200;
        public int clean_cycles { get; set; } = 5;
        public int timeout_blot_motion { get; set; } = 2000;

        public void CopyTo(BioJetProcessConfig copyTo)
        {
            copyTo.prep_delay = prep_delay;
            copyTo.spray_time = spray_time;
            copyTo.blot_time = blot_time;
            copyTo.clean_time = clean_time;
            copyTo.clean_cycles = clean_cycles;
            copyTo.timeout_blot_motion = timeout_blot_motion;
        }
    }

    public class SprayingSystemConfig
    {
        public CameraConfig camera { get; set; }
        public RpiConfig rpi { get; set; }
        public RobotConfig robot { get; set; }
        public BioJetProcessConfig biojet_process { get; set; }


        public static string FileName = @"SprayingSystemConfig.json";

        /// <summary>
        /// This constructor method is required for reflection when loading JSON config.
        /// </summary>
        public SprayingSystemConfig()
        {
        }

        public static SprayingSystemConfig LoadSettings(InMemoryLogProvider logProvider)
        {
            return LoadSettings(FileName, logProvider);
        }

        public static SprayingSystemConfig LoadSettings(string filename, InMemoryLogProvider logProvider)
        {
            SprayingSystemConfig configuration;

            try
            {
                var dataString = File.ReadAllText(filename);
                configuration = JsonSerializer.Deserialize<SprayingSystemConfig>(dataString);
                if (configuration == null)
                {
                    throw new InvalidOperationException("Deserialization resulted in a null configuration object.");
                }
            }
            catch (FileNotFoundException)
            {
                logProvider.CreateLogger(nameof(AppViewModel)).LogError("Configuration file not found: {FileName}", filename);
                throw; // Re-throw to let the caller handle the exception.
            }
            catch (JsonException je)
            {
                logProvider.CreateLogger(nameof(AppViewModel)).LogError(je, "JSON deserialization error for file: {FileName}", filename);
                throw new InvalidOperationException("Configuration could not be loaded due to JSON deserialization error.", je);
            }
            catch (Exception e)
            {
                logProvider.CreateLogger(nameof(AppViewModel)).LogError(e, "Unexpected error loading configuration file: {FileName}", filename);
                throw; // Re-throw to let the caller handle the exception.
            }

            return configuration;
        }

        public void Save(string filename)
        {
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            string jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filename, jsonString);
        }

        public void CopyTo(SprayingSystemConfig copyTo)
        {
            // Don't Copy:
            //  CameraConfig
            //  RpiConfig

            // Copy:
            robot.CopyTo(copyTo.robot);
            biojet_process.CopyTo(copyTo.biojet_process);
        }
    }

    public class SprayingSystemConfigValidate
    {
        public static bool IsValid(SprayingSystemConfig config)
        {
            if (!IsSpeedValid(config))
                return false;

            if (!IsMotionTypeValid(config))
                return false;

            return true;
        }

        private static bool IsSpeedValid(SprayingSystemConfig config)
        {
            foreach (var location in config.robot.locations)
            {
                if (location.speed > 100)
                    return false;

                if (location.speed < 0)
                    return false;
            }

            return true;
        }

        private static bool IsMotionTypeValid(SprayingSystemConfig config)
        {
            foreach (var location in config.robot.locations)
            {
                var mt = location.motiontype;

                if (string.IsNullOrEmpty(mt))
                    continue;

                mt = mt.ToUpper();

                if (mt != "GO"
                    || mt != "JUMP"
                    || mt != "MOVE")
                    return false;
            }

            return true;
        }
    }

}
