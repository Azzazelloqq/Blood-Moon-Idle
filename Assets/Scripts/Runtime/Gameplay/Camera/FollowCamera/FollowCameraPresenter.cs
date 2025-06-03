using System;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Core.Infrastructure.Services.CameraService;
using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.Camera.Base;
using TickHandler;
using UnityEngine;

namespace Runtime.Gameplay.Camera.FollowCamera
{
	public class FollowCameraPresenter : CameraPresenterBase, IGameplayCamera
	{
		public bool IsFollowing => _followCameraModel.IsFollowing;
		public Vector3 Offset => _followCameraModel.Offset;
		public float SmoothTime => _followCameraModel.SmoothTime;
		
		private readonly FollowCameraModel _followCameraModel;
		private ReadOnlyTransform _followTarget;

		public FollowCameraPresenter(
			CameraViewBase view,
			CameraModelBase model,
			ITickHandler tickHandler) : base(view, model, tickHandler)
		{
			_followCameraModel = model as FollowCameraModel;
		}

		protected override ValueTask OnDisposeAsync(CancellationToken token)
		{
			return default;
		}
		
		protected override void OnCameraInitialize()
		{
			base.OnCameraInitialize();

			if (view is FollowCameraView followView)
			{
				_followCameraModel.SetOffset(followView.InitialOffset);
				_followCameraModel.SetSmoothTime(followView.InitialSmoothTime);
				_followCameraModel.SetFollowSpeed(followView.InitialFollowSpeed);
				_followCameraModel.SetRotationSpeed(followView.InitialRotationSpeed);
			}
		}

		protected override void UpdateCamera(float deltaTime)
		{
			if (!model.IsFollowing)
			{
				return;
			}

			_followCameraModel.SetTargetPosition(_followTarget.Position);

			base.UpdateCamera(deltaTime);
		}

		public void SetFollowTarget(ReadOnlyTransform target)
		{
			_followTarget = target;

			_followCameraModel.SetTargetPosition(target.Position);
		}

		public void SetOffset(Vector3 offset)
		{
			_followCameraModel.SetOffset(offset);
			if (view is FollowCameraView followView)
			{
				followView.SetOffset(offset);
			}
		}

		public void SetSmoothTime(float smoothTime)
		{
			_followCameraModel.SetSmoothTime(smoothTime);
			if (view is FollowCameraView followView)
			{
				followView.SetSmoothTime(smoothTime);
			}
		}

		public void SetFollowSpeed(float speed)
		{
			_followCameraModel.SetFollowSpeed(speed);
		}

		public void SetRotationSpeed(float speed)
		{
			_followCameraModel.SetRotationSpeed(speed);
		}

		public void StartFollowing()
		{
			_followCameraModel.StartFollowing();
		}

		public void StopFollowing()
		{
			_followCameraModel.StopFollowing();
		}
	}
}