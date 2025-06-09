using UnityEngine;
using MVP;
using TickHandler;
using LightDI.Runtime;
using BloodMoonIdle.Core.Input;
using BloodMoonIdle.Gameplay.Character.Movement;

namespace BloodMoonIdle.Gameplay.Character
{
    public class CharacterPresenter : Presenter<CharacterView, CharacterModel>
    {
        private readonly IMovementSystem _movementSystem;
        private readonly IInputService _inputService;
        private readonly ITickHandler _tickHandler;
        
        public CharacterPresenter(
            CharacterView view,
            CharacterModel model,
            [Inject] IMovementSystem movementSystem,
            [Inject] IInputService inputService,
            [Inject] ITickHandler tickHandler) : base(view, model)
        {
            _movementSystem = movementSystem;
            _inputService = inputService;
            _tickHandler = tickHandler;
        }
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            _movementSystem.SetModel(model);
            _movementSystem.SetView(view);
            
            // Set initial model position from View
            model.Position = view.transform.position;
            
            // Subscribe to updates
            _tickHandler.SubscribeOnFrameUpdate(OnUpdate);
        }
        
        protected override void OnDispose()
        {
            // Unsubscribe from TickHandler
            _tickHandler?.UnsubscribeOnFrameUpdate(OnUpdate);
            
            base.OnDispose();
        }
        
        private void OnUpdate(float deltaTime)
        {
            // Handle movement
            Vector2 inputDirection = _inputService.MovementDirection;
            _movementSystem.Move(inputDirection, deltaTime);
        }
    }
} 