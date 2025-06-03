using System;
using Runtime.Core.Architecture.CompositionRoot.Base;
using Runtime.Core.Architecture.UI;
using UnityEngine;

namespace Runtime.Core.Architecture.CompositionRoot.Main
{
public class GameEntryPointSceneContext : MonoBehaviour, IRootContext, IDisposable
{
	[SerializeField]
	private UIProvider _uiProvider;
      
	[SerializeField]
	private Transform _rootTransform;
	
	public UIProvider UIProvider => _uiProvider;
	public Transform RootTransform => _rootTransform;

	public void Dispose()
	{
	}
}
}