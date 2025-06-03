using UnityEngine;

namespace Runtime.Core.Architecture.Input
{
    public class InputService : IInputService
    {
        private readonly IInputProvider _inputProvider;
        
        public Vector2 MovementDirection => _inputProvider.MovementDirection;
        public bool IsMoving => _inputProvider.IsActive;

        public InputService(IInputProvider inputProvider)
        {
            _inputProvider = inputProvider;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
    }
} 