using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Runtime.Gameplay.Characters.Player.Base;
using UnityEngine;

namespace Runtime.Gameplay.Characters.Player
{
    public class PlayerModel : PlayerModelBase
    {
        public event System.Action<Vector3> OnPositionChanged;
        public event System.Action<Vector3> OnDirectionChanged;
        public event System.Action<bool> OnIsMovingChanged;

        public override Vector3 Position { get; protected set; }
        public override Vector3 Direction { get; protected set; }
        public override float MovementSpeed { get; protected set; } = 5f;
        public override int CurrentLevel { get; protected set; }
        public override bool IsEnable { get; protected set; }
        public override bool IsDead { get; protected set; }

        private readonly IReadOnlyDictionary<int, float> _moveSpeedByLevel;

        public PlayerModel(IReadOnlyDictionary<int, float> moveSpeedByLevel, int currentLevel) : base()
        {
            _moveSpeedByLevel = moveSpeedByLevel;
            CurrentLevel = currentLevel;
            MovementSpeed = moveSpeedByLevel[CurrentLevel];
        }
        
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

        public override bool CanMove()
        {
            return IsEnable;
        }

        public override void Enable()
        {
            IsEnable = true;
        }

        public override void Disable()
        {
            IsEnable = false;
        }

        public override void LevelUp()
        {
            CurrentLevel++;
            MovementSpeed = _moveSpeedByLevel[CurrentLevel];
        }

        public override void SetDirection(Vector3 direction)
        {
            if (Direction != direction)
            {
                Direction = direction;
                OnDirectionChanged?.Invoke(Direction);
            }
        }

        public override void ProcessMovement(float deltaTime)
        {
            var oldPosition = Position;
            var movement = Direction * MovementSpeed * deltaTime;
            Position += movement;
            
            OnPositionChanged?.Invoke(Position);
        }

        public override void InitializePosition(Vector3 position)
        {
            var oldPosition = Position;
            Position = position;
            
            OnPositionChanged?.Invoke(Position);
        }
    }
} 