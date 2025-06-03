using UnityEngine;

namespace Runtime.Core.Infrastructure.Services.PeopleSpawnService
{
public readonly struct SpawnSettings
{
	public Transform SpawnParent { get; }
	public float MinSpawnDistanceFromPlayer { get; }
	public float MaxSpawnDistanceFromPlayer  { get; }
	public float CameraBehindSpawnDistance  { get; }
	public int SpawnCheckInterval  { get; }
	public int TargetSpawnCount { get; }

	public SpawnSettings(
		Transform spawnParent,
		float minSpawnDistanceFromPlayer,
		float maxSpawnDistanceFromPlayer,
		float cameraBehindSpawnDistance,
		int spawnCheckInterval,
		int targetSpawnCount)
	{
		MinSpawnDistanceFromPlayer = minSpawnDistanceFromPlayer;
		MaxSpawnDistanceFromPlayer = maxSpawnDistanceFromPlayer;
		CameraBehindSpawnDistance = cameraBehindSpawnDistance;
		SpawnCheckInterval = spawnCheckInterval;
		TargetSpawnCount = targetSpawnCount;
		SpawnParent = spawnParent;
	}
}
}