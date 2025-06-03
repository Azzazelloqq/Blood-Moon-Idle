using System;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Config.Remote.DayNightConfig
{
[Serializable]
internal struct LightingPeriod
{
	[SerializeField]
	private TimeOfDay _timeOfDay;

	[SerializeField]
	[Range(1000f, 20000f)]
	private float _temperature;

	[SerializeField]
	[ColorUsage(false, true)]
	private Color _filter;

	[SerializeField]
	[Range(0f, 8f)]
	private float _intensity;

	[SerializeField]
	[Range(0f, 1f)]
	private float _normalizedTimeStart;

	[SerializeField]
	[Range(0f, 1f)]
	private float _normalizedTimeEnd;

	public TimeOfDay TimeOfDay => _timeOfDay;
	public float Temperature => _temperature;
	public Color Filter => _filter;
	public float Intensity => _intensity;
	public float NormalizedTimeStart => _normalizedTimeStart;
	public float NormalizedTimeEnd => _normalizedTimeEnd;

	public LightingPeriod(
		TimeOfDay timeOfDay,
		float temperature,
		Color filter,
		float intensity,
		float normalizedTimeStart,
		float normalizedTimeEnd)
	{
		_timeOfDay = timeOfDay;
		_temperature = Mathf.Clamp(temperature, 1000f, 20000f);
		_filter = filter;
		_intensity = Mathf.Clamp(intensity, 0f, 8f);
		_normalizedTimeStart = Mathf.Clamp01(normalizedTimeStart);
		_normalizedTimeEnd = Mathf.Clamp01(normalizedTimeEnd);
	}
}
}