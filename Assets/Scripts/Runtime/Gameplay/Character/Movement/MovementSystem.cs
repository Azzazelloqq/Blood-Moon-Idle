using UnityEngine;
using LightDI.Runtime;

namespace BloodMoonIdle.Gameplay.Character.Movement
{
    public class MovementSystem : IMovementSystem
    {
        private CharacterModel _model;
        private CharacterView _view;
        
        public MovementSystem()
        {
        }
        
        public void SetModel(CharacterModel model)
        {
            _model = model;
        }
        
        public void SetView(CharacterView view)
        {
            _view = view;
        }
        
        public void Move(Vector2 inputDirection, float deltaTime)
        {
            if (_model == null || _view == null) return;
            
            bool isMoving = inputDirection.magnitude > 0.1f;
            _model.IsMoving = isMoving;
            
            if (isMoving)
            {
                // Convert 2D direction to 3D
                Vector3 direction3D = new Vector3(inputDirection.x, 0, inputDirection.y).normalized;
                _model.Direction = direction3D;
                
                // Calculate new position
                Vector3 movement = direction3D * _model.MovementSpeed * deltaTime;
                _model.Position += movement;
                
                // Update View
                _view.SetPosition(_model.Position);
                _view.SetRotation(direction3D);
            }
            
            // Update animation
            _view.PlayMoveAnimation(isMoving);
        }
    }
} 