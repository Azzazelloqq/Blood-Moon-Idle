using System;
using Azzazelloqq.DetectionService.Source.Debug;
using Runtime.Core.Architecture.CompositionRoot.Base;
using UnityEngine;

namespace Runtime.Core.Architecture.CompositionRoot.Gameplay.Main.Context
{
    public class GameplaySceneContext : MonoBehaviour, IRootContext, IDisposable
    {
        [SerializeField]
        private Transform _characterSpawnPoint;

        #if UNITY_EDITOR
        [SerializeField]
        private DetectionServiceGizmos _detectionServiceGizmos;
        #endif
        public Transform CharacterSpawnPoint => _characterSpawnPoint;
        
        #if UNITY_EDITOR
        public DetectionServiceGizmos DetectionServiceGizmos => _detectionServiceGizmos;
        #endif
        
        public void Dispose()
        {
        }
    }
} 