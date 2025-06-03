using System.Threading;
using System.Threading.Tasks;
using Runtime.Gameplay.Camera.Base;
using UnityEngine;

namespace Runtime.Gameplay.Camera.FollowCamera
{
	public class FollowCameraView : CameraViewBase
	{
		[SerializeField]
		private Vector3 _initialOffset = new(0, 5, -10);

		[SerializeField]
		private float _initialSmoothTime = 0.3f;

		[SerializeField]
		private float _initialFollowSpeed = 5f;

		[SerializeField]
		private float _initialRotationSpeed = 3f;

		public Vector3 InitialOffset => _initialOffset;
		public float InitialSmoothTime => _initialSmoothTime;
		public float InitialFollowSpeed => _initialFollowSpeed;
		public float InitialRotationSpeed => _initialRotationSpeed;

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
		
		protected override ICamera CreateCamera()
		{
			return new UnityCamera(_unityCamera);
		}

		public void SetOffset(Vector3 offset)
		{
			_initialOffset = offset;
		}

		public void SetSmoothTime(float smoothTime)
		{
			_initialSmoothTime = smoothTime;
		}
	}
}