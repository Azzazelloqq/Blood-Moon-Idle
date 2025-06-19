using System.Threading;
using System.Threading.Tasks;
using BloodMoonIdle.UI.Joystick;

namespace BloodMoonIdle.Runtime.UI.Joystick.Base
{
    public interface IJoystickFactory
    {
        public Task<JoystickViewModel> CreateJoystickAsync(CancellationToken token);
    }
} 