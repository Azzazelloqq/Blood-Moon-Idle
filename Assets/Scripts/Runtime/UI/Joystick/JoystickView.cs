using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Azzazelloqq.MVVM.Source.Core.View;
using Azzazelloqq.MVVM.Source.ReactiveLibrary.Callbacks;

namespace BloodMoonIdle.UI.Joystick
{
    public class JoystickView : ViewMonoBehavior<JoystickViewModel>, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private float _handleRange = 1f;
        [SerializeField] private Button _testButton; // Example UI element
        [SerializeField] private Slider _testSlider; // Example UI element
        
        private Subscription<Vector2> _inputVectorSubscription;
        private Subscription<bool> _isPressedSubscription;
        private Subscription<bool> _isActiveSubscription;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // Bind to reactive properties
            BindToViewModel();
            
            // Bind UI elements to commands (if they exist)
            BindUIElements();
        }
        
        private void BindToViewModel()
        {
            // Subscribe to input vector changes
            _inputVectorSubscription = viewModel.InputVector.Subscribe(OnInputVectorChanged);
            
            // Subscribe to pressed state changes  
            _isPressedSubscription = viewModel.IsPressed.Subscribe(OnPressedStateChanged);
            
            // Subscribe to active state changes
            _isActiveSubscription = viewModel.IsActive.Subscribe(OnActiveStateChanged);
            
            // Add subscriptions to composite disposable for automatic cleanup
            compositeDisposable.AddDisposable(_inputVectorSubscription);
            compositeDisposable.AddDisposable(_isPressedSubscription);
            compositeDisposable.AddDisposable(_isActiveSubscription);
        }
        
        private void BindUIElements()
        {
            // Proper command binding without lambdas - subscribe with method references
            if (_testButton != null)
            {
                _testButton.onClick.AddListener(OnTestButtonClicked);
            }
            
            if (_testSlider != null)
            {
                _testSlider.onValueChanged.AddListener(OnTestSliderValueChanged);
            }
        }
        
        protected override void OnDispose()
        {
            // Properly unsubscribe from UI events
            UnbindUIElements();
            
            base.OnDispose();
        }
        
        private void UnbindUIElements()
        {
            // Proper unsubscription - use same method references
            if (_testButton != null)
            {
                _testButton.onClick.RemoveListener(OnTestButtonClicked);
            }
            
            if (_testSlider != null)
            {
                _testSlider.onValueChanged.RemoveListener(OnTestSliderValueChanged);
            }
        }
        
        // UI event handlers - method references for performance
        private void OnTestButtonClicked()
        {
            // Execute command without lambda allocation
            viewModel.OnPointerUpCommand.Execute();
        }
        
        private void OnTestSliderValueChanged(float value)
        {
            // Execute command with parameter without lambda allocation
            viewModel.SetActiveCommand.Execute(value > 0.5f);
        }
        
        private void OnInputVectorChanged(Vector2 inputVector)
        {
            // Update handle position based on input vector
            Vector2 handlePosition = inputVector * (100f * _handleRange);
            _handle.anchoredPosition = handlePosition;
        }
        
        private void OnPressedStateChanged(bool isPressed)
        {
            // Visual feedback for pressed state if needed
            if (!isPressed)
            {
                _handle.anchoredPosition = Vector2.zero;
            }
        }
        
        private void OnActiveStateChanged(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
        
        // Unity Event System handlers - delegate to ViewModel commands
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
            if (!viewModel.IsPressed.Value) return;
            
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background, 
                eventData.position, 
                eventData.pressEventCamera, 
                out position);
            
            position = Vector2.ClampMagnitude(position, _handleRange * 100f);
            
            Vector2 inputVector = position / (100f * _handleRange);
            viewModel.OnDrag(inputVector);
        }
    }
} 