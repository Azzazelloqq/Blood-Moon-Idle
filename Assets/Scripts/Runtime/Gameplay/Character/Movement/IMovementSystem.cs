using UnityEngine;

namespace BloodMoonIdle.Gameplay.Character.Movement
{
    public interface IMovementSystem
    {
        void Move(Vector2 inputDirection, float deltaTime);
        void SetModel(CharacterModel model);
        void SetView(CharacterView view);
    }
} 