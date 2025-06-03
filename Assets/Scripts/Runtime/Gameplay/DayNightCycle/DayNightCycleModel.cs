using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;
using Runtime.Gameplay.DayNightCycle.Base;
using LocalLightingPeriod = Runtime.Core.Infrastructure.Config.Local.DayNightConfig.LocalLightingPeriod;

namespace Runtime.Gameplay.DayNightCycle
{
public class DayNightCycleModel : DayNightCycleModelBase
{
	public override event Action<LightingSettings> OnLightingChanged;
	
	public override DayPhase CurrentDayPhase { get; protected set; }
	public override float CurrentNormalizedTime { get; protected set; }
	public override bool IsRunning { get; protected set; }
	public override bool IsEnable { get; protected set; }

	private readonly IReadOnlyList<LocalLightingPeriod> _lightingPeriods;

	public DayNightCycleModel(IReadOnlyList<LocalLightingPeriod> lightingPeriods)
	{
		_lightingPeriods = lightingPeriods;
	}

	#region LifeCycle
	protected override void OnInitialize()
	{
	}

	protected override ValueTask OnInitializeAsync(CancellationToken token)
	{
		return default;
	}

	protected override void OnDispose()
	{
	}

	protected override ValueTask OnDisposeAsync(CancellationToken token)
	{
		return default;
	}
	#endregion
	

	public override void StartCycle()
	{
		if (IsRunning)
		{
			return;
		}
		
		IsRunning = true;
	}

	public override void StopCycle()
	{
		if (!IsRunning)
		{
			return;
		}
		
		IsRunning = false;
	}

	public override void Enable()
	{
		IsEnable = true;
	}

	public override void Disable()
	{
		IsEnable = false;
	}

	public override void UpdateTime(DayPhase dayPhase, float normalizedTime)
	{
		if (!IsRunning)
		{
			return;
		}

		if (!IsEnable)
		{
			return;
		}
		
		CurrentDayPhase = dayPhase;
		CurrentNormalizedTime = normalizedTime;

		UpdateLighting();
	}

	private void UpdateLighting()
	{
		var normalizedTime = CurrentNormalizedTime;

		foreach (var period in _lightingPeriods)
		{
			if (!(normalizedTime >= period.NormalizedTimeStart) || !(normalizedTime < period.NormalizedTimeEnd))
			{
				continue;
			}

			if (period.DayPhase == CurrentDayPhase)
			{
				var lightingSettings = new LightingSettings(
					period.DayPhase,
					period.Temperature,
					period.Filter,
					period.Intensity,
					normalizedTime
				);

				OnLightingChanged?.Invoke(lightingSettings);
			}

			break;
		}
	}
}
}