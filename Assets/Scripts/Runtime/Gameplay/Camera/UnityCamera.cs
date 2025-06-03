using Runtime.Gameplay.Camera.Base;
using UnityEngine;

namespace Runtime.Gameplay.Camera
{
    /// <summary>
    /// Unity Camera implementation of ICamera abstraction
    /// </summary>
    public class UnityCamera : ICamera
    {
        private readonly UnityEngine.Camera _unityCamera;
        private readonly Transform _transform;

        public UnityCamera(UnityEngine.Camera unityCamera)
        {
            _unityCamera = unityCamera;
            _transform = unityCamera.transform;
        }

        public Transform Transform => _transform;

        public Vector3 Position
        {
            get => _transform.position;
            set => _transform.position = value;
        }

        public Quaternion Rotation
        {
            get => _transform.rotation;
            set => _transform.rotation = value;
        }

        public Vector3 Forward => _transform.forward;
        public Vector3 Right => _transform.right;
        public Vector3 Up => _transform.up;

        public void LookAt(Vector3 target)
        {
            _transform.LookAt(target);
        }

        public void LookAt(Transform target)
        {
            _transform.LookAt(target);
        }

        public void SetParent(Transform parent)
        {
            _transform.SetParent(parent);
        }

        public void SetFieldOfView(float fov)
        {
            if (_unityCamera.orthographic == false)
            {
                _unityCamera.fieldOfView = fov;
            }
        }

        public void SetOrthographicSize(float size)
        {
            if (_unityCamera.orthographic)
            {
                _unityCamera.orthographicSize = size;
            }
        }
    }
} 