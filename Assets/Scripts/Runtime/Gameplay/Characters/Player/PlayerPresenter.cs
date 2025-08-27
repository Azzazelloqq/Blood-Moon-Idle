using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.Config;
using Azzazelloqq.DetectionService.Source;
using LightDI.Runtime;
using Runtime.Core.Architecture.Input;
using Runtime.Core.Infrastructure.Config.Local.PlayerConfig;
using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.Characters.Player.Base;
using TickHandler;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Player
{
    public class PlayerPresenter : PlayerPresenterBase
    {
        public override ReadOnlyTransform CharacterTransform => view.Transform;

        private readonly IInputService _inputService;
        private readonly IConfig _config;
        private readonly ITickHandler _tickHandler;
        private readonly IDetectionService _detectionService;
        private PlayerConfigPage _playerConfig;

        public PlayerPresenter(
            PlayerViewBase view,
            PlayerModelBase model,
            [Inject] IInputService inputService,
            [Inject] IConfig config,
            [Inject] ITickHandler tickHandler,
            [Inject] IDetectionService detectionService) 
            : base(view, model)
        {
            _inputService = inputService;
            _config = config;
            _tickHandler = tickHandler;
            _detectionService = detectionService;

            if (_config.IsInitialized)
            {
                _playerConfig = _config.GetConfigPage<PlayerConfigPage>();
            }
        }
        
        protected override void OnInitialize()
        {
            SubscribeOnModelEvents();
            
            _detectionService.RegisterObject(view);
            
            _tickHandler.SubscribeOnFrameUpdate(OnUpdate);
        }

        protected override ValueTask OnInitializeAsync(CancellationToken token)
        {
            SubscribeOnModelEvents();
            
            _detectionService.RegisterObject(view);
            
            _tickHandler.SubscribeOnFrameUpdate(OnUpdate);

            return default;
        }

        protected override void OnDispose()
        {
            _detectionService.UnregisterObject(view);
            
            _tickHandler.UnsubscribeOnFrameUpdate(OnUpdate);

            UnsubscribeOnModelEvents();
        }

        protected override ValueTask OnDisposeAsync(CancellationToken token)
        {
            _detectionService.UnregisterObject(view);
            
            return default;
        }

        public override void InitializePosition(Vector3 position)
        {
            var oldPosition = model.Position;
            
            model.InitializePosition(position);
            view.SetPosition(model.Position);
            
            _detectionService.UpdateObjectPosition(view, oldPosition);
        }

        public override void Enable()
        {
            model.Enable();
            
            view.SetActive(model.IsEnable);
        }

        public override void Disable()
        {
            model.Disable();
            
            view.SetActive(model.IsEnable);
        }

        public override void UpdateParent(Transform parent)
        {
            view.SetParent(parent);
        }

        private void OnUpdate(float deltaTime)
        {
            ProcessMovement(deltaTime);
        }

        private void ProcessMovement(float deltaTime)
        {
            var inputDirection = _inputService.MovementDirection;

            var direction = new Vector3(inputDirection.x, 0, inputDirection.y).normalized;

            model.SetDirection(direction);

            if (!model.CanMove())
            {
                return;
            }

            var oldPosition = model.Position;
            
            model.ProcessMovement(deltaTime);
            ApplyMovementToView(deltaTime);
            
            _detectionService.UpdateObjectPosition(view, oldPosition);
        }

        private void SubscribeOnModelEvents()
        {
            var playerModel = model as PlayerModel;
            if (playerModel != null)
            {
                playerModel.OnDirectionChanged += OnModelDirectionChanged;
                playerModel.OnIsMovingChanged += OnModelMovingStateChanged;
            }
        }

        private void UnsubscribeOnModelEvents()
        {
            var playerModel = model as PlayerModel;
            if (playerModel != null)
            {
                playerModel.OnDirectionChanged -= OnModelDirectionChanged;
                playerModel.OnIsMovingChanged -= OnModelMovingStateChanged;
            }
        }

        private void ApplyMovementToView(float deltaTime)
        {
            view.UpdatePosition(model.Position, deltaTime);
            view.UpdateRotation(model.Direction);
        }

        private void OnModelDirectionChanged(Vector3 direction)
        {
            view.UpdateRotation(direction);
        }

        private void OnModelMovingStateChanged(bool isMoving)
        {
            view.UpdateMovementState(isMoving);
        }
    }
} 