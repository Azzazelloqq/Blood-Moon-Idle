using System;
using UnityEngine;

namespace Runtime.Core.Architecture.Input
{
    public interface IInputService : IDisposable
    {
        Vector2 MovementDirection { get; }
        bool IsMoving { get; }
        
        void Initialize();
    }
} 