using Runtime.Core.Infrastructure.TransformUtils;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Services.CameraService
{
    public interface IGameplayCamera
    {
        public bool IsFollowing { get; }
        public Vector3 Offset { get; }
        public float SmoothTime { get; }
        
        public void SetFollowTarget(ReadOnlyTransform target);
        public void StartFollowing();
        public void StopFollowing();
        public void SetOffset(Vector3 offset);
        public void SetSmoothTime(float smoothTime);
        public void SetFollowSpeed(float speed);
        public void SetRotationSpeed(float speed);
    }
} 