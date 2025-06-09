using UnityEngine;
using MVP;

namespace BloodMoonIdle.Gameplay.Character
{
    public class CharacterView : ViewMonoBehaviour<CharacterPresenter>
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private string _moveAnimationParameter = "IsMoving";
        
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
        
        public void SetRotation(Vector3 direction)
        {
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        public void PlayMoveAnimation(bool isMoving)
        {
            if (_animator != null)
            {
                _animator.SetBool(_moveAnimationParameter, isMoving);
            }
        }
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            // Additional View initialization
        }
    }
} 