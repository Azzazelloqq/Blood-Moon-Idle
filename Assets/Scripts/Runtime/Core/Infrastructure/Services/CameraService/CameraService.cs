using Runtime.Core.Infrastructure.TransformUtils;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Services.CameraService
{
    public class CameraService : ICameraService
    {
        private IGameplayCamera _gameplayCamera;

        public bool HasActiveCamera => _gameplayCamera != null;

        public void SetGameplayCamera(IGameplayCamera camera)
        {
            _gameplayCamera = camera;
        }

        public void SetFollowTarget(ReadOnlyTransform target)
        {
            _gameplayCamera.SetFollowTarget(target);
        }

        public void StartFollowing()
        {
            _gameplayCamera.StartFollowing();
        }

        public void StopFollowing()
        {
            _gameplayCamera.StopFollowing();
        }

        public bool IsFollowing => _gameplayCamera.IsFollowing;

        public void SetFollowOffset(Vector3 offset)
        {
            _gameplayCamera.SetOffset(offset);
        }

        public void SetFollowSpeed(float speed)
        {
            _gameplayCamera.SetFollowSpeed(speed);
        }

        public void SetRotationSpeed(float speed)
        {
            _gameplayCamera.SetRotationSpeed(speed);
        }
    }
} 