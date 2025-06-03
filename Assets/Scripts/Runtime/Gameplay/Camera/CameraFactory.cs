using LightDI.Runtime;
using Runtime.Gameplay.Camera.FollowCamera;
using TickHandler;

namespace Runtime.Gameplay.Camera
{
    public class CameraFactory : ICameraFactory
    {
        private readonly ITickHandler _tickHandler;

        public CameraFactory([Inject] ITickHandler tickHandler)
        {
            _tickHandler = tickHandler;
        }

        public FollowCameraPresenter CreateFollowCameraPresenter(FollowCameraView view)
        {
            var model = CreateFollowCameraModel();
            return new FollowCameraPresenter(view, model, _tickHandler);
        }

        public FollowCameraModel CreateFollowCameraModel()
        {
            return new FollowCameraModel();
        }
    }
} 