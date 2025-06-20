using UnityEngine;

namespace BloodMoonIdle.Runtime.Core.Architecture.UI
{
	public interface IUIProvider
	{
		public T CreateInCanvas<T>(T prefab) where T : Component;
		public void MoveToCanvas<T>(T targetToMove) where T : Component;
		public void MoveToCanvas(Transform targetToMove);
		public T CreateInCanvas<T>(GameObject prefab) where T : Component;
	}
}