using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;
using UnityEngine;

namespace Runtime.Gameplay.DayNightCycle
{
public struct LightingSettings
{
	public DayPhase DayPhase { get; }
	public float Temperature { get; }
	public Color Filter { get; }
	public float Intensity { get; }
	public float NormalizedTime { get; }

	public LightingSettings(DayPhase dayPhase, float temperature, Color filter, float intensity, float normalizedTime)
	{
		DayPhase = dayPhase;
		Temperature = temperature;
		Filter = filter;
		Intensity = intensity;
		NormalizedTime = normalizedTime;
	}
}
}