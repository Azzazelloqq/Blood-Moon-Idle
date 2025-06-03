using System.Threading;
using System.Threading.Tasks;
using Runtime.Gameplay.Camera.Base;
using UnityEngine;

namespace Runtime.Gameplay.Camera.FollowCamera
{
    public class FollowCameraModel : CameraModelBase
    {
        private Vector3 _offset = new Vector3(0, 5, -10);
        private float _smoothTime = 0.3f;
        private Vector3 _velocity;

        public Vector3 Offset => _offset;
        public float SmoothTime => _smoothTime;

        protected override void OnInitialize()
        {
        }

        protected override ValueTask OnInitializeAsync(CancellationToken token)
        {
            return default;
        }

        protected override void OnDispose()
        {
        }

        protected override ValueTask OnDisposeAsync(CancellationToken token)
        {
            return default;
        }
        
        public void SetOffset(Vector3 offset)
        {
            _offset = offset;
        }

        public void SetSmoothTime(float smoothTime)
        {
            _smoothTime = smoothTime;
        }

        public override void UpdatePosition(float deltaTime)
        {
            if (_isFollowing)
            {
                Vector3 targetPosition = _targetPosition + _offset;
                Vector3 smoothedPosition = Vector3.SmoothDamp(_position, targetPosition, ref _velocity, _smoothTime);
                SetPosition(smoothedPosition);
            }
        }

        public override void UpdateRotation(float deltaTime)
        {
            if (_isFollowing)
            {
                Vector3 direction = (_targetPosition - _position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    Quaternion smoothedRotation = Quaternion.Slerp(_rotation, targetRotation, _rotationSpeed * deltaTime);
                    SetRotation(smoothedRotation);
                }
            }
        }
    }
} 