using System;
using UnityEngine;

namespace Runtime.Core.Architecture.Input
{
    public interface IInputProvider : IDisposable
    {
        public Vector2 MovementDirection { get; }
        public bool IsActive { get; }
    }
} 