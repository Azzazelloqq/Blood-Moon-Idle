using Runtime.Gameplay.Camera.FollowCamera;

namespace Runtime.Gameplay.Camera
{
    /// <summary>
    /// Factory interface for creating camera components with dependency injection
    /// </summary>
    public interface ICameraFactory
    {
        FollowCameraPresenter CreateFollowCameraPresenter(FollowCameraView view);
        FollowCameraModel CreateFollowCameraModel();
    }
} 