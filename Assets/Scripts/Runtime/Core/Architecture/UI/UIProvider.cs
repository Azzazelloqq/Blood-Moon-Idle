using UnityEngine;

namespace BloodMoonIdle.Runtime.Core.Architecture.UI
{
	public class UIProvider : MonoBehaviour, IUIProvider
	{
		[SerializeField]
		private Canvas _mainCanvas;

		public T CreateInCanvas<T>(T prefab) where T : Component
		{
			var created = Instantiate(prefab, _mainCanvas.transform);
			
			return created;
		}
		
		public T CreateInCanvas<T>(GameObject prefab) where T : Component
		{
			var created = Instantiate(prefab, _mainCanvas.transform).GetComponent<T>();
			
			return created;
		}

		public void MoveToCanvas<T>(T targetToMove) where T : Component
		{
			targetToMove.transform.SetParent(_mainCanvas.transform);
		}
		
		public void MoveToCanvas(Transform targetToMove)
		{
			targetToMove.SetParent(_mainCanvas.transform);
		}
	}
}