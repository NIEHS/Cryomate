using System;
using System.Collections.Generic;

namespace SprayingSystem.Models
{
    public class RobotVariablesModel
    {
        public string Date;
        public string SprayTime;
        public string PrepTime;
        public string BlotTime;
        public string CleanTime;
        public string CleanCycles;
        public string TimeoutBlotMotion;

        public static string DateVarName = "Date";

        public static string SprayTimeVarName = "Spray_time";
        public static string PrepDelayVarName = "Prep_delay";
        public static string BlotTimeVarName = "Blot_time";
        public static string CleanTimeVarName = "Clean_time";
        public static string CleanCyclesVarName = "Clean_cycles";
        public static string TimeoutBlotMotionName = "Timeout_blot_motion";


        public void SetValues(List<Tuple<string, string>> values)
        {
            foreach (var pair in values)
            {
                if (pair.Item1.Equals(SprayTimeVarName))
                    SprayTime = pair.Item2;
                else if (pair.Item1.Equals(PrepDelayVarName))
                    PrepTime = pair.Item2;
                else if (pair.Item1.Equals(BlotTimeVarName))
                    BlotTime = pair.Item2;
                else if (pair.Item1.Equals(CleanTimeVarName))
                    CleanTime = pair.Item2;
                else if (pair.Item1.Equals(CleanCyclesVarName))
                    CleanCycles = pair.Item2;
                else if (pair.Item1.Equals(DateVarName))
                    Date = pair.Item2;
                else if (pair.Item1.Equals(TimeoutBlotMotionName))
                    TimeoutBlotMotion = pair.Item2;
            }
        }

        public void Initialize(Dictionary<string, string> variables, BioJetProcessConfig process)
        {
            variables.Add(RobotVariablesModel.DateVarName, DateTime.Now.ToString());

            variables.Add(RobotVariablesModel.SprayTimeVarName, process.spray_time.ToString());
            variables.Add(RobotVariablesModel.PrepDelayVarName, process.prep_delay.ToString());
            variables.Add(RobotVariablesModel.BlotTimeVarName, process.blot_time.ToString());
            variables.Add(RobotVariablesModel.CleanTimeVarName, process.clean_time.ToString());
            variables.Add(RobotVariablesModel.CleanCyclesVarName, process.clean_cycles.ToString());
            variables.Add(RobotVariablesModel.TimeoutBlotMotionName, process.timeout_blot_motion.ToString());

            variables.Add("g_CycleCount", string.Empty);
            variables.Add("g_CyclesToRun", string.Empty);
            variables.Add("g_EventNumber", string.Empty);
        }
    }

}
