using System;
using MVP;
using UnityEngine;

namespace Runtime.Gameplay.Camera.Base
{
    public abstract class CameraModelBase : Model
    {
        protected Vector3 _position;
        protected Quaternion _rotation;
        protected Vector3 _targetPosition;
        protected bool _isFollowing;
        protected float _followSpeed = 5f;
        protected float _rotationSpeed = 3f;

        public Vector3 Position => _position;
        public Quaternion Rotation => _rotation;
        public Vector3 TargetPosition => _targetPosition;
        public bool IsFollowing => _isFollowing;
        public float FollowSpeed => _followSpeed;
        public float RotationSpeed => _rotationSpeed;

        public event Action<Vector3> OnPositionChanged;
        public event Action<Quaternion> OnRotationChanged;
        public event Action<Vector3> OnTargetPositionChanged;
        public event Action<bool> OnFollowingStateChanged;
        public event Action<float> OnFollowSpeedChanged;
        public event Action<float> OnRotationSpeedChanged;

        public virtual void SetPosition(Vector3 position)
        {
            if (_position != position)
            {
                _position = position;
                OnPositionChanged?.Invoke(_position);
            }
        }

        public virtual void SetRotation(Quaternion rotation)
        {
            if (_rotation != rotation)
            {
                _rotation = rotation;
                OnRotationChanged?.Invoke(_rotation);
            }
        }

        public virtual void SetTargetPosition(Vector3 targetPosition)
        {
            if (!IsFollowing)
            {
                return;
            }
            
            if (_targetPosition != targetPosition)
            {
                _targetPosition = targetPosition;
                OnTargetPositionChanged?.Invoke(_targetPosition);
            }
        }

        public virtual void StartFollowing()
        {
            if (_isFollowing)
            {
                return;
            }
            
            _isFollowing = true;
            OnFollowingStateChanged?.Invoke(_isFollowing);
        }

        public void StopFollowing()
        {
            if (!_isFollowing)
            {
                return;
            }
            
            _isFollowing = false;
            OnFollowingStateChanged?.Invoke(_isFollowing);
        }
        
        public virtual void SetFollowSpeed(float speed)
        {
            if (!Mathf.Approximately(_followSpeed, speed))
            {
                _followSpeed = speed;
                OnFollowSpeedChanged?.Invoke(_followSpeed);
            }
        }

        public virtual void SetRotationSpeed(float speed)
        {
            if (!Mathf.Approximately(_rotationSpeed, speed))
            {
                _rotationSpeed = speed;
                OnRotationSpeedChanged?.Invoke(_rotationSpeed);
            }
        }

        public virtual void UpdatePosition(float deltaTime)
        {
            if (_isFollowing)
            {
                Vector3 newPosition = Vector3.Lerp(_position, _targetPosition, _followSpeed * deltaTime);
                SetPosition(newPosition);
            }
        }

        public virtual void UpdateRotation(float deltaTime)
        {
            if (_isFollowing)
            {
                Vector3 direction = (_targetPosition - _position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    Quaternion newRotation = Quaternion.Lerp(_rotation, targetRotation, _rotationSpeed * deltaTime);
                    SetRotation(newRotation);
                }
            }
        }
    }
} 