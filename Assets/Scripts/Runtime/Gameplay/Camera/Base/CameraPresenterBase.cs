using System.Threading;
using System.Threading.Tasks;
using MVP;
using TickHandler;
using UnityEngine;

namespace Runtime.Gameplay.Camera.Base
{
    public abstract class CameraPresenterBase : Presenter<CameraViewBase, CameraModelBase>
    {
        protected readonly ITickHandler _tickHandler;
        
        protected CameraPresenterBase(
            CameraViewBase view,
            CameraModelBase model,
            ITickHandler tickHandler) : base(view, model)
        {
            _tickHandler = tickHandler;
        }

        protected override void OnInitialize()
        {
            SubscribeOnModelEvents();
            
            model.SetPosition(view.transform.position);
            model.SetRotation(view.transform.rotation);

            _tickHandler.SubscribeOnFrameUpdate(OnUpdate);

            OnCameraInitialize();
        }

        protected override ValueTask OnInitializeAsync(CancellationToken token)
        {
            SubscribeOnModelEvents();
            
            model.SetPosition(view.transform.position);
            model.SetRotation(view.transform.rotation);

            _tickHandler.SubscribeOnFrameUpdate(OnUpdate);

            OnCameraInitialize();
            
            return default;
        }

        protected override void OnDispose()
        {
            _tickHandler.UnsubscribeOnFrameUpdate(OnUpdate);

            UnsubscribeOnModelEvents();
            
            OnCameraDispose();
        }

        protected virtual void OnUpdate(float deltaTime)
        {
            UpdateCamera(deltaTime);
        }

        protected virtual void UpdateCamera(float deltaTime)
        {
            if (!model.IsFollowing)
            {
                return;
            }
            
            model.UpdatePosition(deltaTime);
            model.UpdateRotation(deltaTime);
        }

        protected virtual void SubscribeOnModelEvents()
        {
            model.OnPositionChanged += OnModelPositionChanged;
            model.OnRotationChanged += OnModelRotationChanged;
            model.OnTargetPositionChanged += OnModelTargetPositionChanged;
            model.OnFollowingStateChanged += OnModelFollowingStateChanged;
        }

        protected virtual void UnsubscribeOnModelEvents()
        {
            model.OnPositionChanged -= OnModelPositionChanged;
            model.OnRotationChanged -= OnModelRotationChanged;
            model.OnTargetPositionChanged -= OnModelTargetPositionChanged;
            model.OnFollowingStateChanged -= OnModelFollowingStateChanged;
        }

        protected virtual void OnModelPositionChanged(Vector3 position)
        {
            view.SetPosition(position);
        }

        protected virtual void OnModelRotationChanged(Quaternion rotation)
        {
            view.SetRotation(rotation);
        }

        protected virtual void OnModelTargetPositionChanged(Vector3 targetPosition)
        {
        }

        protected virtual void OnModelFollowingStateChanged(bool isFollowing)
        {
        }

        protected virtual void OnCameraInitialize()
        {
        }

        protected virtual void OnCameraDispose()
        {
        }

        protected ITickHandler TickHandler => _tickHandler;
    }
} 