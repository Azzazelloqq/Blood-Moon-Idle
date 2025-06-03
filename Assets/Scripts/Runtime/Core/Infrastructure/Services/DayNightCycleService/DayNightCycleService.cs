using System;
using System.Collections.Generic;
using LightDI.Runtime;
using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;
using TickHandler;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Services.DayNightCycleService
{
public class DayNightCycleService : IDayNightCycleService, IDisposable
{
	public event Action<DayPhase> OnTimeOfDayChanged;

	public DayPhase CurrentDayPhase { get; private set; }

	public float CurrentNormalizedTime => _totalCycleDurationMilliseconds > 0
		? CurrentCycleTimeMilliseconds / _totalCycleDurationMilliseconds
		: 0f;

	public float CurrentCycleTimeMilliseconds { get; private set; }

	public bool IsRunning => _isRunning && !_isPaused;

	private readonly ITickHandler _tickHandler;
	private readonly float _totalCycleDurationMilliseconds;
	private readonly IReadOnlyList<TimeOfDayPeriod> _timeOfDayPeriods;

	private bool _isRunning;
	private bool _isPaused;
	private DayPhase _lastDayPhase;

	public DayNightCycleService([Inject] ITickHandler tickHandler, float totalCycleDurationMilliseconds, IReadOnlyList<TimeOfDayPeriod> timeOfDayPeriods)
	{
		_tickHandler = tickHandler;
		_totalCycleDurationMilliseconds = totalCycleDurationMilliseconds;
		_timeOfDayPeriods = timeOfDayPeriods;
		CurrentDayPhase = DayPhase.Dawn;
		_lastDayPhase = DayPhase.Dawn;
	}

	public void Dispose()
	{
		OnTimeOfDayChanged = null;
	}

	public void StartCycle()
	{
		if (_isRunning)
		{
			return;
		}
		
		_isRunning = true;
		_isPaused = false;
		CurrentCycleTimeMilliseconds = 0f;
		UpdateTimeOfDay();

		_tickHandler.FrameUpdate += UpdateCycle;
	}

	public void StopCycle()
	{
		if (!_isRunning)
		{
			return;
		}
		
		_isRunning = false;
		_isPaused = false;
		CurrentCycleTimeMilliseconds = 0f;
		
		_tickHandler.FrameUpdate -= UpdateCycle;
	}

	public void PauseCycle()
	{
		_isPaused = true;
	}

	public void ResumeCycle()
	{
		_isPaused = false;
	}

	public void SetTime(float normalizedTime)
	{
		normalizedTime = Mathf.Clamp01(normalizedTime);
		CurrentCycleTimeMilliseconds = normalizedTime * _totalCycleDurationMilliseconds;
		UpdateTimeOfDay();
	}

	public void UpdateCycle(float deltaTimeTime)
	{
		if (!IsRunning)
		{
			return;
		}

		var deltaTimeMilliseconds = deltaTimeTime * 1000;

		CurrentCycleTimeMilliseconds += deltaTimeMilliseconds;

		if (CurrentCycleTimeMilliseconds >= _totalCycleDurationMilliseconds)
		{
			CurrentCycleTimeMilliseconds = 0f;
		}

		UpdateTimeOfDay();
	}

	private void UpdateTimeOfDay()
	{
		var newTimeOfDay = CalculateTimeOfDay(CurrentCycleTimeMilliseconds);

		if (newTimeOfDay == _lastDayPhase)
		{
			return;
		}

		CurrentDayPhase = newTimeOfDay;
		OnTimeOfDayChanged?.Invoke(CurrentDayPhase);
		_lastDayPhase = CurrentDayPhase;
	}

	private DayPhase CalculateTimeOfDay(float currentTimeMilliseconds)
	{
		var normalizedTime = currentTimeMilliseconds / _totalCycleDurationMilliseconds;

		foreach (var period in _timeOfDayPeriods)
		{
			if (normalizedTime >= period.NormalizedTimeStart && normalizedTime < period.NormalizedTimeEnd)
			{
				return period.DayPhase;
			}
		}

		return DayPhase.Dawn;
	}
}
}