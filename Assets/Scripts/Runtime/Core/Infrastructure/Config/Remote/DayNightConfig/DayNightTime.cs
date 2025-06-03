using System;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Config.Remote.DayNightConfig
{
[Serializable]
internal struct DayNightTime
{
	[SerializeField]
	[Range(0, 23)]
	private int _hours;

	[SerializeField]
	[Range(0, 59)]
	private int _minutes;

	[SerializeField]
	[Range(0, 59)]
	private int _seconds;

	public int Hours => _hours;
	public int Minutes => _minutes;
	public int Seconds => _seconds;

	public float TotalSeconds => _hours * 3600f + _minutes * 60f + _seconds;
	public float TotalMilliseconds => TotalSeconds * 1000f;

	public DayNightTime(int hours, int minutes, int seconds)
	{
		_hours = Mathf.Clamp(hours, 0, 23);
		_minutes = Mathf.Clamp(minutes, 0, 59);
		_seconds = Mathf.Clamp(seconds, 0, 59);
	}
}
}