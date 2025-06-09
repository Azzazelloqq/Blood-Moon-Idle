using UnityEngine;

namespace BloodMoonIdle.Core.Input
{
    public interface IInputProvider
    {
        Vector2 MovementDirection { get; }
        bool IsActive { get; }
    }
} 