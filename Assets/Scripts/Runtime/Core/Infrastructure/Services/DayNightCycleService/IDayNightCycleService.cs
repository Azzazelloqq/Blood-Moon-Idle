using System;
using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;

namespace Runtime.Core.Infrastructure.Services.DayNightCycleService
{
	public interface IDayNightCycleService : IDisposable
	{
		public event Action<DayPhase> OnTimeOfDayChanged;
		
		public DayPhase CurrentDayPhase { get; }
		public float CurrentNormalizedTime { get; }
		public float CurrentCycleTimeMilliseconds { get; }
		public bool IsRunning { get; }
		
		public void StartCycle();
		public void StopCycle();
		public void PauseCycle();
		public void ResumeCycle();
		public void SetTime(float normalizedTime);
		public void UpdateCycle(float deltaTimeMilliseconds);
	}
} 