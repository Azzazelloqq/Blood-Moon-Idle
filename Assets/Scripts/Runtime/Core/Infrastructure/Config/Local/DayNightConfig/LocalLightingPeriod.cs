using UnityEngine;

namespace Runtime.Core.Infrastructure.Config.Local.DayNightConfig
{
public struct LocalLightingPeriod
{
	public DayPhase DayPhase { get; }
	public float Temperature { get; }
	public Color Filter { get; }
	public float Intensity { get; }
	public float NormalizedTimeStart { get; }
	public float NormalizedTimeEnd { get; }

	public LocalLightingPeriod(
		DayPhase dayPhase,
		float temperature,
		Color filter,
		float intensity,
		float normalizedTimeStart,
		float normalizedTimeEnd)
	{
		DayPhase = dayPhase;
		Temperature = temperature;
		Filter = filter;
		Intensity = intensity;
		NormalizedTimeStart = normalizedTimeStart;
		NormalizedTimeEnd = normalizedTimeEnd;
	}
}
}