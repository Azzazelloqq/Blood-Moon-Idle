using UnityEngine;
using MVP;
using LightDI.Runtime;

namespace BloodMoonIdle.Gameplay.Character
{
    public class CharacterModel : Model
    {
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public float MovementSpeed { get; set; } = 5f;
        public bool IsMoving { get; set; }
        
        public CharacterModel()
        {
        }
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            // Additional model initialization
        }
    }
} 