using System;
using Runtime.Core.Architecture.CompositionRoot.Base;
using UnityEngine;

namespace Runtime.Core.Architecture.CompositionRoot.Gameplay.City
{
public class CityRootContext : MonoBehaviour, IRootContext, IDisposable
{
	[SerializeField]
	private Transform _playerParent;

	[SerializeField]
	private Transform _dayNightViewParent;
	
	[SerializeField]
	private Transform _sceneParent;

	[SerializeField]
	private Transform _citizensSpawnParent;
	
	public Transform PlayerParent => _playerParent;
	public Transform DayNightViewParent => _dayNightViewParent;
	public Transform SceneParent => _sceneParent;
	public Transform CitizensSpawnParent => _citizensSpawnParent;
	
	public void Dispose()
	{
	}
}
}