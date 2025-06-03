using System;
using Runtime.Core.Architecture.CompositionRoot.Base;
using UnityEngine;

namespace Runtime.Core.Architecture.CompositionRoot.Gameplay.Crypt
{
public class CryptRootContext : MonoBehaviour, IRootContext, IDisposable
{
	[SerializeField]
	private SubSceneContext _subSceneContext;
	
	[SerializeField]
	private Transform _playerParent;

	[SerializeField]
	private Transform _sceneParent;

	public Transform PlayerParent => _playerParent;
	public Transform SceneParent => _sceneParent;
	
	public void Dispose()
	{
	}
}
}