using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;

namespace Runtime.Core.Infrastructure.Services.DayNightCycleService
{
public class TimeOfDayPeriod
{
	public DayPhase DayPhase { get; }
	public float NormalizedTimeStart { get; }
	public float NormalizedTimeEnd { get; }

	public TimeOfDayPeriod(DayPhase dayPhase, float normalizedTimeStart, float normalizedTimeEnd)
	{
		DayPhase = dayPhase;
		NormalizedTimeStart = normalizedTimeStart;
		NormalizedTimeEnd = normalizedTimeEnd;
	}
}
}