using System;
using Runtime.Gameplay.Characters.Base;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Services.PeopleSpawnService
{
public interface IPeopleSpawnService : IDisposable
{
	/// <summary>
	/// Event triggered when a character is spawned
	/// </summary>
	event Action<ICharacter> OnCharacterSpawned;
	
	/// <summary>
	/// Event triggered when a character is consumed/destroyed
	/// </summary>
	event Action<ICharacter> OnCharacterConsumed;

	/// <summary>
	/// Initializes the spawn service with specified target count
	/// </summary>
	/// <param name="targetPeopleCount">Target number of people to maintain on the map</param>
	/// <param name="spawnSettings">Settings of spawn</param>
	void Initialize(SpawnSettings spawnSettings);

	/// <summary>
	/// Gets current active people count
	/// </summary>
	/// <returns>Number of active people on the map</returns>
	int GetActivePeopleCount();

	/// <summary>
	/// Manually triggers spawn of a single character
	/// </summary>
	void SpawnCharacter();

	/// <summary>
	/// Removes a character from tracking (called when character is consumed)
	/// </summary>
	/// <param name="character">Character that was consumed</param>
	void RemoveCharacter(ICharacter character);

	void Enable();
	void Disable();
}
}
