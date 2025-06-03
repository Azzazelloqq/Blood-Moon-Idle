using System.Threading;
using System.Threading.Tasks;
using LightDI.Runtime;
using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;
using Runtime.Core.Infrastructure.Services.DayNightCycleService;
using Runtime.Gameplay.DayNightCycle.Base;
using TickHandler;

namespace Runtime.Gameplay.DayNightCycle
{
public class DayNightCyclePresenter : DayNightCyclePresenterBase
{
	public DayPhase CurrentDayPhase => _dayNightCycleService.CurrentDayPhase;
	public float CurrentNormalizedTime => _dayNightCycleService.CurrentNormalizedTime;
	public bool IsRunning => _dayNightCycleService.IsRunning;

	private readonly IDayNightCycleService _dayNightCycleService;

	public DayNightCyclePresenter(
		DayNightCycleViewBase view,
		DayNightCycleModelBase model,
		[Inject] IDayNightCycleService dayNightCycleService,
		[Inject] ITickHandler tickHandler) : base(view, model, tickHandler)
	{
		_dayNightCycleService = dayNightCycleService;
	}

	protected override void OnInitialize()
	{
		SubscribeToModelEvents();
	}

	protected override ValueTask OnInitializeAsync(CancellationToken token)
	{
		SubscribeToModelEvents();

		return default;
	}

	protected override void OnDispose()
	{
		UnsubscribeFromModelEvents();
	}

	protected override ValueTask OnDisposeAsync(CancellationToken token)
	{
		UnsubscribeFromModelEvents();
		return default;
	}

	public override void StartCycle()
	{
		if (model.IsRunning)
		{
			return;
		}

		TickHandler.FrameUpdate += OnUpdate;

		SyncWithService();

		model.StartCycle();
	}

	public override void StopCycle()
	{
		if (!model.IsRunning)
		{
			return;
		}

		model.StopCycle();
		TickHandler.FrameUpdate -= OnUpdate;
	}

	public override void Enable()
	{
		model.Enable();

		view.SetActive(model.IsEnable);
	}

	public override void Disable()
	{
		model.Disable();
		
		view.SetActive(model.IsEnable);
	}

	private void SyncWithService()
	{
		view.UpdateDayPhase(_dayNightCycleService.CurrentDayPhase, _dayNightCycleService.CurrentNormalizedTime);
		model.UpdateTime(_dayNightCycleService.CurrentDayPhase, _dayNightCycleService.CurrentNormalizedTime);
	}

	private void SubscribeToModelEvents()
	{
		model.OnLightingChanged += OnLightingChanged;
	}

	private void UnsubscribeFromModelEvents()
	{
		model.OnLightingChanged -= OnLightingChanged;
	}

	private void OnUpdate(float deltaTime)
	{
		model.UpdateTime(_dayNightCycleService.CurrentDayPhase, _dayNightCycleService.CurrentNormalizedTime);
	}

	private void OnLightingChanged(LightingSettings lightingSettings)
	{
		var deltaTime = TickHandler.DeltaTime;
		view.ApplyLightingSettings(lightingSettings, deltaTime);
	}
}
}