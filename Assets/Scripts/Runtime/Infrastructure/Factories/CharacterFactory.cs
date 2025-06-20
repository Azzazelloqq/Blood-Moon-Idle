using UnityEngine;
using LightDI.Runtime;
using ResourceLoader;
using TickHandler;
using InGameLogger;
using BloodMoonIdle.Gameplay.Character;
using BloodMoonIdle.Gameplay.Character.Movement;
using BloodMoonIdle.Core.Input;
using BloodMoonIdle.Infrastructure.Services;

namespace BloodMoonIdle.Infrastructure.Factories
{
    public class CharacterFactory : ICharacterFactory
    {
        private const string CHARACTER_PREFAB_ID = "Hero";
        private readonly IResourceLoader _resourceLoader;
        private readonly IInGameLogger _logger;
        
        public CharacterFactory([Inject] IResourceLoader resourceLoader, [Inject] IInGameLogger logger)
        {
            _resourceLoader = resourceLoader;
            _logger = logger;
        }
        
        public CharacterPresenter CreateCharacter(Vector3 position)
        {
            try
            {
                var characterPrefab = _resourceLoader.LoadResource<GameObject>(CHARACTER_PREFAB_ID);
                var characterGameObject = Object.Instantiate(characterPrefab, position, Quaternion.identity);
                
                var characterView = characterGameObject.GetComponent<CharacterView>();
                if (characterView == null)
                {
                    _logger.LogError($"[{LogTags.CHARACTER_FACTORY}] Character prefab {CHARACTER_PREFAB_ID} doesn't have CharacterView component!");
                    return null;
                }
                
                var characterModel = new CharacterModel();
                
                var presenter = CharacterPresenterFactory.CreateCharacterPresenter(characterView, characterModel);
                presenter.Initialize();
                
                _logger.Log($"[{LogTags.CHARACTER_FACTORY}] Character created successfully at {position}");
                return presenter;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[{LogTags.CHARACTER_FACTORY}] Failed to create character: {ex.Message}");
                _logger.LogException(ex);
                
                return null;
            }
        }
    }
} 