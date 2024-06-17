namespace SprayingSystem.RobotDriver
{
    public interface IRobotDriver
    {
        bool Start();
        void TurnMotorsOn();
        bool GoHome();
        void TurnMotorsOff();
        void SetAccelDecelWithDefaults();
        void Stop();
        bool GoStandby();
        bool GoBeforeSpray();
        bool GoAfterSpray();

        bool GoFrontBlot();
        bool GoBackBlot();
        bool blotSolenoid();
        bool blotSolenoidFwd();
        bool blotSolenoidBack();

        bool sprayOn();

        bool sprayOff();

        bool GoGridStorePosition(int position);

        void SetSpeed(int speed);
        void SetDefaultSpeed(int speed);
        void SetMotionType(MotionType motionType);
    }
}
