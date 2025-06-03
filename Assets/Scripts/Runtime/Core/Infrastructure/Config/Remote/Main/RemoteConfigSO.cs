using Runtime.Core.Infrastructure.Config.Remote.DayNightConfig;
using Runtime.Core.Infrastructure.Config.Remote.PlayerConfig;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Config.Remote.Main
{
[CreateAssetMenu(fileName = "MainRemoteConfig", menuName = "BloodMoonIdle/Config/MainRemoteConfig", order = 1)]
public class RemoteConfigSO : ScriptableObject
{
	[SerializeField]
	private PlayerRemoteConfigPage _playerRemoteConfigPage;

	[SerializeField]
	private DayNightCycleRemoteConfigPage _dayNightCycleRemoteConfigPage;

	internal PlayerRemoteConfigPage PlayerRemoteConfigPage => _playerRemoteConfigPage;
	internal DayNightCycleRemoteConfigPage DayNightCycleRemoteConfigPage => _dayNightCycleRemoteConfigPage;
}
}