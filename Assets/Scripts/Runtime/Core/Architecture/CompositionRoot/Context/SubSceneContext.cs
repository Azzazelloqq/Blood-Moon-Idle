using System;
using UnityEngine;

namespace Runtime.Core.Architecture.CompositionRoot.Gameplay
{
[Serializable]
public struct SubSceneContext
{
	[SerializeField]
	private Transform _subSceneParent;

	[SerializeField]
	private GameObject _subScenePrefab;
	
	public Transform SubSceneParent => _subSceneParent;
	public GameObject SubScenePrefab => _subScenePrefab;
}
}