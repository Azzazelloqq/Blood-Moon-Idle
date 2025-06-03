using MVP;
using UnityEngine;

namespace Runtime.Gameplay.Camera.Base
{
    public abstract class CameraViewBase : ViewMonoBehaviour<CameraPresenterBase>
    {
        [SerializeField] protected UnityEngine.Camera _unityCamera;
        
        protected ICamera mainCamera;

        protected virtual void Awake()
        {
            if (_unityCamera == null)
            {
                _unityCamera = GetComponent<UnityEngine.Camera>();
            }
            
            InitializeCamera();
        }

        protected virtual void InitializeCamera()
        {
            mainCamera = CreateCamera();
        }

        protected abstract ICamera CreateCamera();

        public virtual void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public virtual void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public virtual void LookAt(Vector3 target)
        {
            transform.LookAt(target);
        }

        protected UnityEngine.Camera UnityCamera => _unityCamera;
    }
} 