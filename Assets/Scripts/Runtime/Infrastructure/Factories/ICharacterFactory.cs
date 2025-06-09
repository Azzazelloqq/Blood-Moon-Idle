using UnityEngine;
using BloodMoonIdle.Gameplay.Character;

namespace BloodMoonIdle.Infrastructure.Factories
{
    public interface ICharacterFactory
    {
        CharacterPresenter CreateCharacter(Vector3 position);
    }
} 