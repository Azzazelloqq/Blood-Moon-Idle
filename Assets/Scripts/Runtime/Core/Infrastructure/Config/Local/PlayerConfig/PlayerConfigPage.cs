using System.Collections.Generic;
using Azzazelloqq.Config;

namespace Runtime.Core.Infrastructure.Config.Local.PlayerConfig
{
public struct PlayerConfigPage : IConfigPage
{
	public IReadOnlyDictionary<int, float> MoveSpeedByLevel { get; }
	public float RotationSpeed { get; }

	public PlayerConfigPage(Dictionary<int, float> moveSpeedByLevel, float rotationSpeed)
	{
		RotationSpeed = rotationSpeed;
		MoveSpeedByLevel = moveSpeedByLevel;
	}
}
}