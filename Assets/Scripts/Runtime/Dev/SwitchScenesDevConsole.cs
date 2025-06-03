using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Dev
{
public class SwitchScenesDevConsole : MonoBehaviour
{
	#if UNITY_EDITOR
	
	public static event Action<string> SwitchToScene;
	
	[SerializeField]
	private Button _switchToCryptScene;

	[SerializeField]
	private Button _switchToCityScene;

	private void OnEnable()
	{
		_switchToCityScene.onClick.AddListener(OnSwitchToCitySceneButtonClicked);
		_switchToCryptScene.onClick.AddListener(OnSwitchToCryptSceneButtonClicked);
	}

	private void OnDisable()
	{
		_switchToCityScene.onClick.RemoveListener(OnSwitchToCitySceneButtonClicked);
		_switchToCryptScene.onClick.RemoveListener(OnSwitchToCryptSceneButtonClicked);
	}

	private void OnSwitchToCitySceneButtonClicked()
	{
		SwitchToScene?.Invoke("City");
	}

	private void OnSwitchToCryptSceneButtonClicked()
	{
		SwitchToScene?.Invoke("Crypt");
	}


	#endif
}
}