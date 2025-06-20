using UnityEngine;
using LightDI.Runtime;
using BloodMoonIdle.Core.Input;

namespace BloodMoonIdle.Infrastructure.Services
{
    public class InputService : IInputService
    {
        private IInputProvider _inputProvider;
        
        public Vector2 MovementDirection => _inputProvider?.MovementDirection ?? Vector2.zero;
        public bool IsMoving => _inputProvider?.IsActive ?? false;
        
        public InputService()
        {
        }
        
        public void Initialize()
        {
        }
        
        public void SetInputProvider(IInputProvider inputProvider)
        {
            _inputProvider = inputProvider;
        }
    }
} 