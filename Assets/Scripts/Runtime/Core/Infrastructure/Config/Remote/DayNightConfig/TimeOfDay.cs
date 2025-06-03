using System;

namespace Runtime.Core.Infrastructure.Config.Remote.DayNightConfig
{
[Serializable]
internal enum TimeOfDay
{
	Dawn = 0,
	Day = 1,
	Noon = 2,
	Dusk = 3,
	Night = 4,
	Midnight = 5
}
}