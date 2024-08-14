using System;
using System.Threading;
using SprayingSystem.Models;
using SprayingSystem.ViewModels;
// using Python.Runtime;

namespace SprayingSystem.SprayingProcess
{
    public class SprayAndPlungeProcess
    {
        public static void Process(
            ProcessOptionsViewModel ProcessOptionsViewModel,
            RobotVariablesViewModel RobotVariablesViewModel,
            RobotVariablesModel RobotVariablesModel,
            RobotViewModel RobotViewModel,
            CameraViewModel CameraViewModel,
            InMemoryLogProvider logProvider)
        {
            logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("Process: Prepare And Plunge");

            if (ProcessOptionsViewModel.RecordSpray)
            {
                CameraViewModel.StartRecordingCmd.Execute(null);
            }

            /*
             * Move to the spray position
             * Blot solenoid forward
             * Two Options:
             * Fast Spray true in ProcessOptionsViewModel: Spray on for spray_time
             * Slow Spray true in ProcessOptionsViewModel: Spray on after prep_delay for spray_time
             * Blot soleoid backward
             * Move to plunge position
             */

            // this time should be only the spray time since preparation delay came from RPi
            int totalWaitTime = TotalTimeToWaitForSpraying(ProcessOptionsViewModel, RobotVariablesModel);
            int prepDelay = GetPrepDelay(ProcessOptionsViewModel, RobotVariablesModel);

            if (ProcessOptionsViewModel.Spray)
            {
                logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("Process: Spraying");

                RobotViewModel.MoveToSprayPositionCmd.Execute(null);

                Thread.Sleep(1000);

                RobotViewModel.blotSolenoidFwdCommand(null);

                Thread.Sleep(totalWaitTime);

                if(ProcessOptionsViewModel.Spray_FastSpray)
                {
                    RobotViewModel.sprayOnCommand(null);
                    Thread.Sleep(int.Parse(RobotVariablesModel.SprayTime));
                    RobotViewModel.sprayOffCommand(null);
                }
                else
                {
                    Thread.Sleep(prepDelay);
                    RobotViewModel.sprayOnCommand(null);
                    Thread.Sleep(int.Parse(RobotVariablesModel.SprayTime));
                    RobotViewModel.sprayOffCommand(null);
                }


                Thread.Sleep(int.Parse(RobotVariablesModel.BlotTime));
                RobotViewModel.blotSolenoidBackCommand(null);
            }

            if (ProcessOptionsViewModel.Blot)
            {
                int timeoutBlotMotion = int.Parse(RobotVariablesModel.TimeoutBlotMotion);

                if (ProcessOptionsViewModel.Blot_BackBlot)
                {
                    logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("Process: Back Blotting");
                    RobotViewModel.MoveToBackBlotPosition(null);
                    Thread.Sleep(timeoutBlotMotion);

                    RobotViewModel.blotSolenoidFwdCommand(null);

                    // Wait for the solenoid to actuate.
                    Thread.Sleep(int.Parse(RobotVariablesModel.BlotTime));

                    RobotViewModel.blotSolenoidBackCommand(null);
                }
                else
                {
                    // ProcessOptionsViewModel.Blot_GridToBlot
                    logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("Process: Front Blotting");
                    RobotViewModel.MoveToFrontBlotPosition(null);
                    Thread.Sleep(timeoutBlotMotion);

                    //RpiViewModel.BlotSolenoidCmd.Execute(null);
                    //RobotViewModel.blotSolenoidCommand(null);
                    RobotViewModel.blotSolenoidFwdCommand(null);

                    Thread.Sleep(int.Parse(RobotVariablesModel.BlotTime));

                    RobotViewModel.blotSolenoidBackCommand(null);

                    RobotViewModel.MoveToBackBlotPosition(null);
                }
            }
            /*
            if (ProcessOptionsViewModel.SonicateTweezers)
            {
                Runtime.PythonDLL = @"C:\Users\dandeyvp\AppData\Local\Programs\Python\Python39\python39.dll";
                logger("Process: Mix");
                //RpiViewModel.SprayCmd.Execute(null);  
                //Thread.Sleep(totalWaitTime);
                PythonEngine.Initialize();

                using (Py.GIL())
                {
                    //dynamic mixtest = Py.Import("Call_func_gen");

                    //mixtest.Call_func_gen();
                    //Modify the Python path if necessary
                    dynamic sys = Py.Import("sys");
                    sys.path.append("C:\\Users\\dandeyvp\\Desktop\\python_modules");

                    //Import the Python module
                    dynamic Call_func_gen = Py.Import("Call_func_gen");


                }
                PythonEngine.Shutdown();

                //RpiViewModel.BlotSolenoidReverseCmd.Execute(null);
                //Thread.Sleep(int.Parse(RobotVariablesModel.BlotTime));
            }
            */
            RobotViewModel.MoveToPlungePosition(null);


            if (ProcessOptionsViewModel.RecordSpray)
            {
                CameraViewModel.StopRecordingCmd.Execute(null);
                CameraViewModel.SaveVideoCmd.Execute(null);
                logProvider.CreateLogger(nameof(AppViewModel)).LogInformation("Process: Video recorded and saved.");
            }


            /*
            if (ProcessOptionsViewModel.RpiRecordSpray)
            {
                // TODO: How to make sure the image was acquired and it is saved on file.
                Thread.Sleep(RpiViewModel.VideoSavingDelayMillisec);
                RpiViewModel.GetVideoFileCmd.Execute(null);
                // Play video from here?
                RpiViewModel.PlayVideoCmd.Execute(null);
            }
            */

            RobotVariablesViewModel.WriteToLog();
        }

        public static int TotalTimeToWaitForSpraying(
            ProcessOptionsViewModel processOptionsViewModel,
            RobotVariablesModel robotVariablesModel)
        {
            int totalWaitTime = 0;

            if (processOptionsViewModel.Spray)
            {
                totalWaitTime += GetSprayTime(processOptionsViewModel, robotVariablesModel);
            }

            //totalWaitTime += GetBlotTime(processOptionsViewModel, robotVariablesModel);

            return totalWaitTime;
        }

        private static int GetSprayTime(
            ProcessOptionsViewModel processOptionsViewModel,
            RobotVariablesModel robotVariablesModel)
        {
            if (processOptionsViewModel.Spray)
            {
                if (!string.IsNullOrEmpty(robotVariablesModel.SprayTime))
                    return int.Parse(robotVariablesModel.SprayTime);
            }

            return 0;
        }

        public static int GetPrepDelay(
        ProcessOptionsViewModel processOptionsViewModel,
        RobotVariablesModel robotVariablesModel)
        {
            if (processOptionsViewModel.Spray)
            {
                if (!string.IsNullOrEmpty(robotVariablesModel.PrepTime))
                    return int.Parse(robotVariablesModel.SprayTime);
            }

            return 0;
        }

        private static int GetBlotTime(
            ProcessOptionsViewModel processOptionsViewModel,
            RobotVariablesModel robotVariablesModel)
        {
            if (processOptionsViewModel.Blot)
            {
                if (!string.IsNullOrEmpty(robotVariablesModel.BlotTime))
                    return int.Parse(robotVariablesModel.BlotTime);
            }

            return 0;
        }
    }
}
