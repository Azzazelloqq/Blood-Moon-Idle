using MVP;
using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;
using UnityEngine;

namespace Runtime.Gameplay.DayNightCycle.Base
{
	public abstract class DayNightCycleViewBase : ViewMonoBehaviour<DayNightCyclePresenterBase>
	{
		[Header("Animation Settings")]
		[SerializeField] protected float _transitionSpeed = 1f;

		public abstract void ApplyLightingSettings(LightingSettings lightingSettings, float deltaTime);
		public abstract void UpdateDayPhase(DayPhase dayPhase, float normalizedTime);
		public abstract void SetActive(bool isActive);
	}
} 