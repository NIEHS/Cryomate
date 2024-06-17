using System;
using System.Threading;
using System.Windows;
using RCAPINet;
using SprayingSystem.Models;
using SprayingSystem.ViewModels;

namespace SprayingSystem.RobotDriver
{
    public class EpsonRc7Driver : IRobotDriver
    {
        #region Fields

        private Spel _spel;
        private EpsonRc7DriverConfig _config;
        private readonly InMemoryLogProvider _logProvider;

        #endregion

        public Spel Spel
        {
            get { return _spel; }
        }

        public EpsonRc7Driver(EpsonRc7DriverConfig config, InMemoryLogProvider logProvider)
        {
            _config = config;
            _logProvider = logProvider ?? throw new ArgumentNullException(nameof(logProvider));
        }

        public bool Start()
        {
            try
            {
                _spel = new Spel();
                _spel.Initialize();

                _spel.Project = _config.RobotProject;
                _spel.Connect(_config.RobotConfigurationNumber);

                _spel.EventReceived += SpelOnEventReceived;

                _spel.EnableEvent(SpelEvents.AllTasksStopped, true);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public void TurnMotorsOn()
        {
            if (_spel == null)
                return;

            _spel.MotorsOn = true;
            Thread.Sleep(2000);

            if (!_spel.PowerHigh)
            {
                _spel.PowerHigh = true;
                Thread.Sleep(2000);
            }

        }

        public void SetAccelDecelWithDefaults()
        {
            // While this is the global default, each move can change
            // the speed value and hence leaving this command useless.
            // _spel.Speed(_config.DefaultSpeed);
            // Thread.Sleep(1000);

            // These are useful because each move command doesn't set these
            // values. Only the global settings are being used (as v1.6.0)
            _spel.Accel(_config.DefaultAccel, _config.DefaultDecel);
            Thread.Sleep(1000);
        }

        public void TurnMotorsOff()
        {
            if (_spel == null)
                return;

            _spel.MotorsOn = false;
        }

        public void Stop()
        {
            // TODO: Don't forget to call Stop when we close out.

            if (_spel == null)
                return;

            _spel.Dispose();
            _spel = null;
        }

        public void OpenSimulatorWindow()
        {
            try
            {
                _spel.ShowWindow(RCAPINet.SpelWindows.Simulator);
            }
            catch (Exception e)
            {
                _logProvider.CreateLogger(nameof(AppViewModel)).LogError(e.Message);
            }
        }

        public bool GoHome()
        {
            if (_spel == null || !_spel.MotorsOn)
                return false;

            MoveToPointNumber(PointNames.Home, _config.MotionType);
            return true;
        }

        public bool GoStandby()
        {
            if (_spel == null || !_spel.MotorsOn)
                return false;

            MoveToPointNumber(PointNames.Tweezers, _config.MotionType);
            return true;
        }

        public bool GoBeforeSpray()
        {
            if (_spel == null || !_spel.MotorsOn)
                return false;

            MoveToPointNumber(PointNames.Spray, _config.MotionType);
            return true;
        }

        public bool GoAfterSpray()
        {
            if (_spel == null || !_spel.MotorsOn)
                return false;

            MoveToPointNumber(PointNames.Plunge, _config.MotionType);
            return true;
        }

        public bool GoBackBlot()
        {
            if (_spel == null || !_spel.MotorsOn)
                return false;

            MoveToPointNumber(PointNames.BackBlot, _config.MotionType);
            return true;
        }

        public bool GoFrontBlot()
        {
            if (_spel == null || !_spel.MotorsOn)
                return false;

            MoveToPointNumber(PointNames.FrontBlot, _config.MotionType);
            return true;
        }

        public bool blotSolenoidFwd()
        {
            //_spel.On(robotVariablesModel.blotPort);
            //Thread.Sleep(1000);
            //_spel.Off(robotVariablesModel.blotPort);

            _spel.On(0);
            //Thread.Sleep(int.Parse(RobotVariablesModel.BlotTime));
            //_spel.Off(1);

            return true;
        }

        public bool blotSolenoidBack()
        {
            //_spel.On(robotVariablesModel.blotPort);
            //Thread.Sleep(1000);
            //_spel.Off(robotVariablesModel.blotPort);

            _spel.Off(0);
            //Thread.Sleep(int.Parse(RobotVariablesModel.BlotTime));
            //_spel.Off(1);

            return true;
        }
        public bool blotSolenoid()
        {
            //_spel.On(robotVariablesModel.blotPort);
            //Thread.Sleep(1000);
            //_spel.Off(robotVariablesModel.blotPort);

            //_spel.Off(1);
            //Thread.Sleep(int.Parse(RobotVariablesModel.BlotTime));
            //_spel.Off(1);

            return true;
        }

        public bool sprayOn()
        {
            //_spel.On(robotVariablesModel.sprayPort);
            //Thread.Sleep(1000);
            //_spel.Off(robotVariablesModel.sprayPort);

            _spel.On(2);

            return true;
        }

        public bool sprayOff()
        {
            //_spel.On(robotVariablesModel.sprayPort);
            //Thread.Sleep(1000);
            //_spel.Off(robotVariablesModel.sprayPort);

            _spel.Off(2);

            return true;
        }

        public bool GoGridStorePosition(int position)
        {
            if (_spel == null || !_spel.MotorsOn)
                return false;

            PointNames gridPointName;
            switch (position)
            {
                case 1:
                    gridPointName = PointNames.GridStore1;
                    break;
                case 2:
                    gridPointName = PointNames.GridStore2;
                    break;
                case 3:
                    gridPointName = PointNames.GridStore3;
                    break;
                case 4:
                    gridPointName = PointNames.GridStore4;
                    break;
                case 5:
                    gridPointName = PointNames.GridStore5;
                    break;
                case 6:
                    gridPointName = PointNames.GridStore6;
                    break;
                case 7:
                    gridPointName = PointNames.GridStore7;
                    break;
                case 8:
                    gridPointName = PointNames.GridStore8;
                    break;
                default:
                    gridPointName = PointNames.GridStore1;
                    break;
            }

            MoveToPointNumber(gridPointName, MotionType.JUMP, _config.GridStoreJumpParameters.command);

            return true;
        }

        public void SetMotionType(MotionType motionType)
        {
            _config.MotionType = motionType;
        }

        public void SetDefaultSpeed(int speed)
        {
            if (_spel == null)
                return;

            _config.DefaultSpeed = speed;
        }

        public void SetSpeed(int speed)
        {
            if (_spel == null)
                return;

            _spel.Speed(speed);
            Thread.Sleep(10);
        }

        public void Reset()
        {
            try
            {
                _spel.Reset();
            }
            catch (Exception e)
            {
                _logProvider.CreateLogger(nameof(AppViewModel)).LogError(e.Message);
            }
        }

        #region Private

        private void SpelOnEventReceived(object sender, SpelEventArgs e)
        {
            if (e.Event == SpelEvents.AllTasksStopped)
            {

            }
        }

        private void MoveToPointNumber(RobotDriver.PointNames pointName, MotionType defaultMotionType, string command = "")
        {
            // TODO: We have to make sure that previous move was completed (or previous API call finished).
            // Otherwise we will get an error.

            // Convert the robot number to the user's robot position number
            var robotLocation = _config.GetRobotPositionInfo(pointName.ToString());

            // Set Speed if defined.
            if (robotLocation.speed > 0)
                SetSpeed(robotLocation.speed);

            _spel.Accel(100, 100);
            //_spel.Accel(_config.DefaultAccel, _config.DefaultDecel);

            // Map to the robot point number defined in the config.
            var robotPoint = robotLocation.point;

            // Use default motion type if one is not defined.
            var motionType = GetMotionTypeForMove(robotLocation, defaultMotionType);

            bool useCommand = false;
            var commandParameter = string.Empty;
            if (!string.IsNullOrEmpty(command))
            {
                useCommand = true;
                commandParameter = $"P{robotLocation.point} {command}";
            }

            // Perform the actual motion.
            switch (motionType)
            {
                case MotionType.GO:
                    if (useCommand)
                        _spel.Go(commandParameter);
                    else
                        _spel.Go(robotPoint);
                    break;

                case MotionType.JUMP:
                    if (useCommand)
                        _spel.Jump(commandParameter);
                    else
                        _spel.Jump(robotPoint);
                    break;

                case MotionType.MOVE:
                    if (useCommand)
                        _spel.Move(commandParameter);
                    else
                        _spel.Move(robotPoint);
                    break;
            }
        }

        private MotionType GetMotionTypeForMove(RobotLocation robotLocation, MotionType defaultMotionType)
        {
            var motionType = defaultMotionType;
            string motionTypeString = string.Empty;
            if (robotLocation.motiontype != null)
                motionTypeString = robotLocation.motiontype.ToUpper();
            if (!string.IsNullOrEmpty(motionTypeString))
                motionType = _config.GetMotionTypeFromString(motionTypeString);

            return motionType;
        }

        #endregion
    }
}
