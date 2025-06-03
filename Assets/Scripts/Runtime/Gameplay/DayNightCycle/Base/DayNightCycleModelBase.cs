using System;
using MVP;
using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;

namespace Runtime.Gameplay.DayNightCycle.Base
{
public abstract class DayNightCycleModelBase : Model
{
	public abstract event Action<LightingSettings> OnLightingChanged;

	public abstract DayPhase CurrentDayPhase { get; protected set; }
	public abstract float CurrentNormalizedTime { get; protected set; }
	public abstract bool IsRunning { get; protected set; }
	public abstract bool IsEnable { get; protected set; }

	public abstract void UpdateTime(DayPhase dayPhase, float normalizedTime);
	public abstract void StartCycle();
	public abstract void StopCycle();
	public abstract void Enable();
	public abstract void Disable();
}
}