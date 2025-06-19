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
		private float _handleRange = 1f;

		private Subscription<Vector2> _inputVectorSubscription;
		private Subscription<bool> _isPressedSubscription;
		private Subscription<bool> _isActiveSubscription;

		protected override void OnInitialize()
		{
			base.OnInitialize();

			BindToViewModel();
		}

		private void BindToViewModel()
		{
			_inputVectorSubscription = viewModel.InputVector.Subscribe(OnInputVectorChanged);
			_isPressedSubscription = viewModel.IsPressed.Subscribe(OnPressedStateChanged);
			_isActiveSubscription = viewModel.IsActive.Subscribe(OnActiveStateChanged);

			compositeDisposable.AddDisposable(_inputVectorSubscription, _isPressedSubscription, _isActiveSubscription);
		}

		private void OnInputVectorChanged(Vector2 inputVector)
		{
			var handlePosition = inputVector * (100f * _handleRange);
			_handle.anchoredPosition = handlePosition;
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

		public void OnPointerDown(PointerEventData eventData)
		{
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
				_background,
				eventData.position,
				eventData.pressEventCamera,
				out var position);

			position = Vector2.ClampMagnitude(position, _handleRange * 100f);

			var inputVector = position / (100f * _handleRange);
			viewModel.OnDrag(inputVector);
		}
	}
}