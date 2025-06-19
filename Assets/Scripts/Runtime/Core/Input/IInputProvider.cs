using UnityEngine;

namespace BloodMoonIdle.Core.Input
{
    public interface IInputProvider
    {
        public Vector2 MovementDirection { get; }
        public bool IsActive { get; }
    }
} 