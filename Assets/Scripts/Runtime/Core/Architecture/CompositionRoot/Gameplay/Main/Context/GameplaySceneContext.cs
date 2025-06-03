using System;
using Runtime.Core.Architecture.CompositionRoot.Base;
using UnityEngine;

namespace Runtime.Core.Architecture.CompositionRoot.Gameplay.Main.Context
{
    public class GameplaySceneContext : MonoBehaviour, IRootContext, IDisposable
    {
        [SerializeField]
        private Transform _characterSpawnPoint;

        public Transform CharacterSpawnPoint => _characterSpawnPoint;
        
        public void Dispose()
        {
        }
    }
} 