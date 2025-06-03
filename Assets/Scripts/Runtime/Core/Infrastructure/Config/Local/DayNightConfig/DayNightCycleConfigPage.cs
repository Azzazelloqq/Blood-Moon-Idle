using System.Collections.Generic;
using Azzazelloqq.Config;

namespace Runtime.Core.Infrastructure.Config.Local.DayNightConfig
{
public struct DayNightCycleConfigPage : IConfigPage
{
	public float DayDurationMilliseconds { get; }
	public float NightDurationMilliseconds { get; }
	public float TotalCycleDurationMilliseconds { get; }
	public IReadOnlyDictionary<DayPhase, LocalLightingPeriod> LightingByTimeOfDay { get; }
	public IReadOnlyList<LocalLightingPeriod> LightingPeriods { get; }
	public bool AutoStart { get; }

	public DayNightCycleConfigPage(
		float dayDurationMilliseconds,
		float nightDurationMilliseconds,
		Dictionary<DayPhase, LocalLightingPeriod> lightingByTimeOfDay,
		List<LocalLightingPeriod> lightingPeriods,
		bool autoStart)
	{
		DayDurationMilliseconds = dayDurationMilliseconds;
		NightDurationMilliseconds = nightDurationMilliseconds;
		TotalCycleDurationMilliseconds = dayDurationMilliseconds + nightDurationMilliseconds;
		LightingByTimeOfDay = lightingByTimeOfDay;
		LightingPeriods = lightingPeriods;
		AutoStart = autoStart;
	}
}
}