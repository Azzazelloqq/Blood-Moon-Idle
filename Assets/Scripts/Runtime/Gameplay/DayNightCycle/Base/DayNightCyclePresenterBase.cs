using MVP;
using TickHandler;

namespace Runtime.Gameplay.DayNightCycle.Base
{
	public abstract class DayNightCyclePresenterBase : Presenter<DayNightCycleViewBase, DayNightCycleModelBase>
	{
		protected ITickHandler TickHandler { get; }

		protected DayNightCyclePresenterBase(
			DayNightCycleViewBase view,
			DayNightCycleModelBase model,
			ITickHandler tickHandler) : base(view, model)
		{
			TickHandler = tickHandler;
		}

		public abstract void StartCycle();
		public abstract void StopCycle();
		public abstract void Enable();
		public abstract void Disable();
	}
} 