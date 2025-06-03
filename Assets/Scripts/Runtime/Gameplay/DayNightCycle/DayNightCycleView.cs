using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;
using Runtime.Gameplay.DayNightCycle.Base;
using UnityEngine;

namespace Runtime.Gameplay.DayNightCycle
{
public class DayNightCycleView : DayNightCycleViewBase
{
	private const float DefaultSunAngleMin = -90f;
	private const float DefaultSunAngleMax = 270f;
	private const float DefaultSunAngleY = 30f;
	private const float DefaultExposureOffset = 0.1f;

	[Header("Lighting References")]
	[SerializeField]
	private Light _mainLight;

	[Header("Sky Settings")]
	[SerializeField]
	private Material _skyboxMaterial;

	[SerializeField]
	private string _skyboxTintProperty = "_Tint";

	[SerializeField]
	private string _skyboxExposureProperty = "_Exposure";

	[Header("Sun Movement")]
	[SerializeField]
	private float _sunAngleMin = DefaultSunAngleMin;

	[SerializeField]
	private float _sunAngleMax = DefaultSunAngleMax;

	[SerializeField]
	private float _sunAngleY = DefaultSunAngleY;

	[Header("Exposure")]
	[SerializeField]
	private float _exposureOffset = DefaultExposureOffset;

	protected override void OnInitialize()
	{
		if (_skyboxMaterial == null)
		{
			_skyboxMaterial = RenderSettings.skybox;
		}
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

	public override void ApplyLightingSettings(LightingSettings lightingSettings, float deltaTime)
	{
		if (_mainLight != null)
		{
			ApplyMainLightSettings(lightingSettings, deltaTime);
		}

		ApplySkyboxSettings(lightingSettings, deltaTime);
	}

	public override void UpdateDayPhase(DayPhase dayPhase, float normalizedTime)
	{
		switch (dayPhase)
		{
			case DayPhase.Dawn:
				ApplyDawnEffects(normalizedTime);
				break;
			case DayPhase.Day:
				ApplyDayEffects(normalizedTime);
				break;
			case DayPhase.Noon:
				ApplyNoonEffects(normalizedTime);
				break;
			case DayPhase.Dusk:
				ApplyDuskEffects(normalizedTime);
				break;
			case DayPhase.Night:
				ApplyNightEffects(normalizedTime);
				break;
			case DayPhase.Midnight:
				ApplyMidnightEffects(normalizedTime);
				break;
		}
	}

	public override void SetActive(bool isActive)
	{
		gameObject.SetActive(isActive);
	}

	private void ApplyMainLightSettings(LightingSettings settings, float deltaTime)
	{
		_mainLight.intensity = Mathf.Lerp(_mainLight.intensity, settings.Intensity, deltaTime * _transitionSpeed);
		_mainLight.color = Color.Lerp(_mainLight.color, settings.Filter, deltaTime * _transitionSpeed);

		var sunAngle = Mathf.Lerp(_sunAngleMin, _sunAngleMax, settings.NormalizedTime);
		_mainLight.transform.rotation = Quaternion.Euler(sunAngle, _sunAngleY, 0f);
	}

	private void ApplySkyboxSettings(LightingSettings settings, float deltaTime)
	{
		if (_skyboxMaterial == null)
		{
			return;
		}

		var targetTint = settings.Filter;
		var targetExposure = Mathf.Log(settings.Intensity + _exposureOffset, 2f);

		if (_skyboxMaterial.HasProperty(_skyboxTintProperty))
		{
			var currentTint = _skyboxMaterial.GetColor(_skyboxTintProperty);
			_skyboxMaterial.SetColor(_skyboxTintProperty,
				Color.Lerp(currentTint, targetTint, deltaTime * _transitionSpeed));
		}

		if (_skyboxMaterial.HasProperty(_skyboxExposureProperty))
		{
			var currentExposure = _skyboxMaterial.GetFloat(_skyboxExposureProperty);
			_skyboxMaterial.SetFloat(_skyboxExposureProperty,
				Mathf.Lerp(currentExposure, targetExposure, deltaTime * _transitionSpeed));
		}
	}

	private void ApplyDawnEffects(float normalizedTime)
	{
	}

	private void ApplyDayEffects(float normalizedTime)
	{
	}

	private void ApplyNoonEffects(float normalizedTime)
	{
	}

	private void ApplyDuskEffects(float normalizedTime)
	{
	}

	private void ApplyNightEffects(float normalizedTime)
	{
	}

	private void ApplyMidnightEffects(float normalizedTime)
	{
	}
}
}