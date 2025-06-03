using System;
using UnityEngine;

namespace Runtime.Core.Infrastructure.Config.Remote.PlayerConfig
{
[Serializable]
internal struct MoveSpeedByLevelRemote
{
	[SerializeField]
	private int _level;

	[SerializeField]
	private float _moveSpeed;

	public int Level => _level;
	public float MoveSpeed => _moveSpeed;
}
}