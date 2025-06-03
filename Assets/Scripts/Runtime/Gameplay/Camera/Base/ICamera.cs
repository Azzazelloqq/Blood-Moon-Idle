using UnityEngine;

namespace Runtime.Gameplay.Camera.Base
{
    /// <summary>
    /// Base camera abstraction that can be extended for different camera types
    /// </summary>
    public interface ICamera
    {
        Transform Transform { get; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        Vector3 Forward { get; }
        Vector3 Right { get; }
        Vector3 Up { get; }
        
        void LookAt(Vector3 target);
        void LookAt(Transform target);
        void SetParent(Transform parent);
        void SetFieldOfView(float fov);
        void SetOrthographicSize(float size);
    }
} 