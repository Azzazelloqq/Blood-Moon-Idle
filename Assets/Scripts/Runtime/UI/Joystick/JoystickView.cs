using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.Source.ReactiveLibrary.Callbacks;
using BloodMoonIdle.UI.Joystick.Base;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BloodMoonIdle.UI.Joystick
{
	public class JoystickView : JoystickViewBase, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		[SerializeField]
		private RectTransform _background;

		[SerializeField]
		private RectTransform _handle;

		[SerializeField]
		private RectTransform _handleZone;

		private float _maxRadius;

		private Subscription<Vector2> _inputVectorSub;
		private Subscription<bool> _isPressedSub;
		private Subscription<bool> _isActiveSub;

		protected override void OnInitialize()
		{
			base.OnInitialize();

			_maxRadius = GetMaxRadius(_background);

			BindToViewModel();
		}

		protected override async Task OnInitializeAsync(CancellationToken token)
		{
			await base.OnInitializeAsync(token);
			
			_maxRadius = GetMaxRadius(_background);

			BindToViewModel();
		}

		private static float GetMaxRadius(RectTransform rect)
		{
			return Mathf.Min(rect.rect.width, rect.rect.height) * 0.5f;
		}


		private void BindToViewModel()
		{
			_inputVectorSub = viewModel.InputVector.Subscribe(OnInputVectorChanged);
			_isPressedSub = viewModel.IsPressed.Subscribe(OnPressedStateChanged);
			_isActiveSub = viewModel.IsActive.Subscribe(OnActiveStateChanged);

			compositeDisposable.AddDisposable(_inputVectorSub, _isPressedSub, _isActiveSub);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_handleZone != null &&
				!RectTransformUtility.RectangleContainsScreenPoint(
					_handleZone, eventData.position, eventData.pressEventCamera))
			{
				return;
			}

			MoveBackgroundTo(eventData);
			viewModel.OnPointerDown(eventData.position);
			OnDrag(eventData); 
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			viewModel.OnPointerUp();
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (!viewModel.IsPressed.Value)
			{
				return;
			}

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_background, eventData.position, eventData.pressEventCamera, out var localPos);

			localPos = Vector2.ClampMagnitude(localPos, _maxRadius);

			var input = localPos / _maxRadius;
			viewModel.OnDrag(input);
		}

		private void OnInputVectorChanged(Vector2 input)
		{
			_handle.anchoredPosition = input * _maxRadius;
		}

		private void OnPressedStateChanged(bool isPressed)
		{
			if (!isPressed)
			{
				_handle.anchoredPosition = Vector2.zero;
			}
		}

		private void OnActiveStateChanged(bool isActive)
		{
			gameObject.SetActive(isActive);
		}

		private void MoveBackgroundTo(PointerEventData eventData)
		{
			if (_handleZone == null)
			{
				return;
			}

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_handleZone, eventData.position, eventData.pressEventCamera, out var localPos);

			var halfBg = new Vector2(_background.rect.width, _background.rect.height) * 0.5f;
			var zone = _handleZone.rect;

			localPos.x = Mathf.Clamp(localPos.x,
				zone.xMin + halfBg.x,
				zone.xMax - halfBg.x);
			localPos.y = Mathf.Clamp(localPos.y,
				zone.yMin + halfBg.y,
				zone.yMax - halfBg.y);

			_background.anchoredPosition = localPos;
		}
	}
}