namespace BloodMoonIdle.UI.Joystick
{
    public interface IJoystickFactory
    {
        JoystickViewModel CreateJoystick(JoystickView view);
    }
} 