using System;
using Runtime.Core.Infrastructure.TransformUtils;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Base
{
public interface ICharacter : IDisposable
{
	ReadOnlyTransform Transform { get; }
	Vector3 Position { get; }
	bool CanBeConsumed { get; }
	bool IsActive { get; }

	void InitializePosition(Vector3 position);
	void Enable();
	void Disable();
	void UpdateParent(Transform parent);
}
}

