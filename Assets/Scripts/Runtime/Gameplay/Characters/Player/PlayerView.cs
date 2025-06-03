using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.DetectionService.Source;
using Runtime.Core.Infrastructure.TransformUtils;
using Runtime.Gameplay.Characters.Player.Base;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Player
{
    public class PlayerView : PlayerViewBase
    {
        [SerializeField] protected Transform _visualRoot;
        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem _movementParticles;
        [SerializeField] private AudioSource _movementAudio;
        [SerializeField] private string _moveAnimationParameter = "IsMoving";
        
        public override Transform VisualRoot => _visualRoot != null ? _visualRoot : transform;
        public override ReadOnlyTransform Transform { get; protected set; }
        public override Vector3 Position => presenter != null ? presenter.CharacterTransform.Position : transform.position;
        public override bool IsDead => false; 

        protected override void OnInitialize()
        {
            Transform = new ReadOnlyTransform(_visualRoot);
        }

        protected override ValueTask OnInitializeAsync(CancellationToken token)
        {
            Transform = new ReadOnlyTransform(_visualRoot);

            return default;
        }
        
        protected override void OnDispose()
        {
        }

        protected override ValueTask OnDisposeAsync(CancellationToken token)
        {
            return default;
        }

        public override void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        public override void UpdatePosition(Vector3 position, float deltaTime)
        {
            VisualRoot.position = position;
        }

        public override void UpdateRotation(Vector3 direction)
        {
            VisualRoot.rotation = Quaternion.LookRotation(direction);
        }

        public override void UpdateMovementState(bool isMoving)
        {
            OnMovementStateChanged(isMoving);
        }

        public override void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public override void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        private void OnMovementStateChanged(bool isMoving)
        {
            PlayMoveAnimation(isMoving);
            PlayMovementEffects(isMoving);
        }

        private void PlayMoveAnimation(bool isMoving)
        {
            if (_animator != null)
            {
                _animator.SetBool(_moveAnimationParameter, isMoving);
            }
        }
        
        private void PlayMovementEffects(bool isMoving)
        {
            if (_movementParticles != null)
            {
                if (isMoving && !_movementParticles.isPlaying)
                {
                    _movementParticles.Play();
                }
                else if (!isMoving && _movementParticles.isPlaying)
                {
                    _movementParticles.Stop();
                }
            }
            
            if (_movementAudio != null)
            {
                if (isMoving && !_movementAudio.isPlaying)
                {
                    _movementAudio.Play();
                }
                else if (!isMoving && _movementAudio.isPlaying)
                {
                    _movementAudio.Stop();
                }
            }
        }
    }
} 