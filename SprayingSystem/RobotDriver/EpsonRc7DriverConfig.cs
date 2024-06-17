using System;
using System.Collections.Generic;
using System.Windows;
using SprayingSystem.Models;
using SprayingSystem.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace SprayingSystem.RobotDriver;

public class EpsonRc7DriverConfig
{
    // 9 setting for Dev / Simulator
    // 1 setting for real robot
    public int RobotConfigurationNumber = 9;

    // This is some kind of project settings to run the robot.
    public string RobotProject = @"C:\EpsonRC70\projects\API_Demos\Demo1\demo1.sprj";

    // Reads these values from the SprayingSystemConfig.json file.
    public int DefaultSpeed = 100;
    public int DefaultAccel = 100;
    public int DefaultDecel = 100;

    private readonly InMemoryLogProvider _logProvider;

    /// <summary>
    /// Access using the enum RobotPoints[PointNames.*]
    /// </summary>
    private Dictionary<string, RobotLocation> RobotPoints;

    public MotionType MotionType = MotionType.GO;

    public GridStoreJumpParameters GridStoreJumpParameters { get; }

    public EpsonRc7DriverConfig(UserOptions userOptions, InMemoryLogProvider logProvider)
    {
        _logProvider = logProvider ?? throw new ArgumentNullException(nameof(logProvider));
        try
        {
            MapUserConfigToPointNames(userOptions);
            GridStoreJumpParameters = userOptions.Options.robot.gridStoreJumpParameters;

            DefaultSpeed = userOptions.Options.robot.defaultMotionParams.speed;
            DefaultAccel = userOptions.Options.robot.defaultMotionParams.accel;
            DefaultDecel = userOptions.Options.robot.defaultMotionParams.decel;
        }
        catch (Exception e)
        {
            _logProvider.CreateLogger(nameof(AppViewModel)).LogError("Fatal Error: Robot requires json configuration file with valid Locations. Application Fatal Error");
            // Application.Current.Shutdown(1);
        }
    }

    public RobotLocation GetRobotPositionInfo(string pointName)
    {
        RobotLocation robotLocation;
        var found = RobotPoints.TryGetValue(pointName, out robotLocation);
        if (!found)
        {
            _logProvider.CreateLogger(nameof(AppViewModel)).LogError("Failed to find Location: " + robotLocation + " Application Fatal Error");
            // Application.Current.Shutdown(1);
        }
        return robotLocation;
    }

    public MotionType GetMotionTypeFromString(string motionTypeString)
    {
        return (MotionType)Enum.Parse(typeof(MotionType), motionTypeString);
    }

    #region Private

    private void MapUserConfigToPointNames(UserOptions userOptions)
    {
        // map  RobotPoints[(int)PointNames.Home] = "Home"

        RobotPoints = new Dictionary<string, RobotLocation>();

        foreach (var locPair in userOptions.Options.robot.locations)
            RobotPoints.Add(locPair.name, locPair);
    }

    #endregion
}