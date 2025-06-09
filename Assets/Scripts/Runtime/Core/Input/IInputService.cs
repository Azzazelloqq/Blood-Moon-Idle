using UnityEngine;

namespace BloodMoonIdle.Core.Input
{
    public interface IInputService
    {
        Vector2 MovementDirection { get; }
        bool IsMoving { get; }
        
        void Initialize();
        void SetInputProvider(IInputProvider inputProvider);
    }
} 