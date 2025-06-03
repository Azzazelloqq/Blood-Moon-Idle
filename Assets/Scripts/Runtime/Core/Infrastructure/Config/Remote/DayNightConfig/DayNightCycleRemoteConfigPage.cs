using System.Collections.Generic;
using Azzazelloqq.Config;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Config.Remote.DayNightConfig
{
	[CreateAssetMenu(fileName = "DayNightCycleRemoteConfigPage",
		menuName = "BloodMoonIdle/Config/RemotePages/DayNightCycleRemoteConfigPage", order = 2)]
	internal class DayNightCycleRemoteConfigPage : ScriptableObject, IConfigPage
	{
		[Header("Day Duration")]
		[SerializeField]
		private DayNightTime _dayDuration = new(0, 5, 0); 

		[Header("Night Duration")]
		[SerializeField]
		private DayNightTime _nightDuration = new(0, 3, 0); 

		[Header("Lighting Periods")]
		[SerializeField]
		private LightingPeriod[] _lightingPeriods = {
			new(TimeOfDay.Dawn, 3500f, Color.white, 0.5f, 0f, 0.125f),
			new(TimeOfDay.Day, 5500f, Color.white, 1f, 0.125f, 0.375f),
			new(TimeOfDay.Noon, 6500f, Color.white, 1.2f, 0.375f, 0.625f),
			new(TimeOfDay.Dusk, 3000f, new Color(1f, 0.6f, 0.3f, 1f), 0.7f, 0.625f, 0.75f),
			new(TimeOfDay.Night, 2000f, new Color(0.3f, 0.4f, 0.8f, 1f), 0.2f, 0.75f, 0.875f),
			new(TimeOfDay.Midnight, 1500f, new Color(0.2f, 0.3f, 0.6f, 1f), 0.1f, 0.875f, 1f)
		};

		[Header("Cycle Settings")]
		[SerializeField]
		private bool _autoStart = true;

		public DayNightTime DayDuration => _dayDuration;
		public DayNightTime NightDuration => _nightDuration;
		public IReadOnlyList<LightingPeriod> LightingPeriods => _lightingPeriods;
		public bool AutoStart => _autoStart;

		public float TotalCycleDuration => _dayDuration.TotalSeconds + _nightDuration.TotalSeconds;
		public float TotalCycleDurationMilliseconds => _dayDuration.TotalMilliseconds + _nightDuration.TotalMilliseconds;
	}
}