using Runtime.Core.Infrastructure.TransformUtils;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Services.CameraService
{
    public interface ICameraService
    {
        bool HasActiveCamera { get; }
        
        void SetGameplayCamera(IGameplayCamera camera);
        void SetFollowTarget(ReadOnlyTransform target);
        void StartFollowing();
        void StopFollowing();
        bool IsFollowing { get; }
        
        void SetFollowOffset(Vector3 offset);
        void SetFollowSpeed(float speed);
        void SetRotationSpeed(float speed);
    }
} 