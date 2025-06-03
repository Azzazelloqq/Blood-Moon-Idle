using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.MVVM.ReactiveLibrary.Callbacks;
using Runtime.UI.Joystick.Base;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.UI.Joystick
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
		private Vector2 _initialBackgroundPosition;

		protected override void OnInitialize()
		{
			_maxRadius = GetMaxRadius(_background);
			_initialBackgroundPosition = _background.anchoredPosition;

			BindToViewModel();
		}

		protected override ValueTask OnInitializeAsync(CancellationToken token)
		{
			_maxRadius = GetMaxRadius(_background);
			_initialBackgroundPosition = _background.anchoredPosition;
			
			BindToViewModel();

			return default;
		}

		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			return default;
		}

		protected override void OnDispose()
		{
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
			SetBackgroundLocalPosition(_initialBackgroundPosition);
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

			SetBackgroundLocalPosition(localPos);
		}

		private void SetBackgroundLocalPosition(Vector2 position)
		{
			_background.anchoredPosition = position;
		}
	}
}