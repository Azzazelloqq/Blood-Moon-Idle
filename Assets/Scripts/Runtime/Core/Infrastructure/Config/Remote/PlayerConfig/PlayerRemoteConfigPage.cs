using System.Collections.Generic;
using Azzazelloqq.Config;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Config.Remote.PlayerConfig
{
[CreateAssetMenu(fileName = "PlayerRemoteConfigPage", menuName = "BloodMoonIdle/Config/RemotePages/PlayerRemoteConfigPage",
	order = 1)]
internal class PlayerRemoteConfigPage : ScriptableObject, IConfigPage
{
	[SerializeField]
	private float _rotationSpeed;

	[SerializeField]
	private MoveSpeedByLevelRemote[] _moveSpeedByLevels;

	public float RotationSpeed => _rotationSpeed;
	public IReadOnlyList<MoveSpeedByLevelRemote> MoveSpeedByLevels => _moveSpeedByLevels;
}
}