using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azzazelloqq.Config;
using Runtime.Core.Infrastructure.Config.Local.DayNightConfig;
using Runtime.Core.Infrastructure.Config.Local.PlayerConfig;
using Runtime.Core.Infrastructure.Config.Remote.DayNightConfig;
using Runtime.Core.Infrastructure.Config.Remote.Main;
using Runtime.Core.Infrastructure.Config.Remote.PlayerConfig;

namespace Runtime.Core.Infrastructure.Config.Parser
{
public class ConfigParser : IConfigParser
{
	private readonly RemoteConfigSO _remoteConfig;

	public ConfigParser(RemoteConfigSO remoteConfig)
	{
		_remoteConfig = remoteConfig;
	}

	public IConfigPage[] Parse()
	{
		var playerConfig = ParsePlayerConfig(_remoteConfig.PlayerRemoteConfigPage);
		var dayNightConfig = ParseDayNightCycleConfig(_remoteConfig.DayNightCycleRemoteConfigPage);
		var configPages = new[]
		{
			playerConfig,
			dayNightConfig
		};

		return configPages;
	}

	public Task<IConfigPage[]> ParseAsync(CancellationToken token)
	{
		var result = Task.Run(() =>
		{
			var playerConfig = ParsePlayerConfig(_remoteConfig.PlayerRemoteConfigPage);
			var dayNightConfig = ParseDayNightCycleConfig(_remoteConfig.DayNightCycleRemoteConfigPage);
			var configPages = new[]
			{
				playerConfig,
				dayNightConfig
			};

			return token.IsCancellationRequested ? null : configPages;
			
		}, token);

		return result;
	}

	public Task<IConfigPage[]> ParseAsync(IProgress<ParseProgress> progress, CancellationToken token)
	{
		var result = Task.Run(() =>
		{
			var playerConfig = ParsePlayerConfig(_remoteConfig.PlayerRemoteConfigPage);
			var dayNightConfig = ParseDayNightCycleConfig(_remoteConfig.DayNightCycleRemoteConfigPage);
			var configPages = new[]
			{
				playerConfig,
				dayNightConfig
			};

			return configPages;
		}, token);

		if (token.IsCancellationRequested)
		{
			return null;
		}

		progress.Report(new ParseProgress(1, string.Empty));
		return result;
	}

	public void ParseAsync(Action<ParseProgress> progress, Action<IConfigPage[]> onParsed, CancellationToken token)
	{
		Task.Run(() =>
		{
			var playerConfig = ParsePlayerConfig(_remoteConfig.PlayerRemoteConfigPage);
			var dayNightConfig = ParseDayNightCycleConfig(_remoteConfig.DayNightCycleRemoteConfigPage);
			var configPages = new[]
			{
				playerConfig,
				dayNightConfig
			};

			if (token.IsCancellationRequested)
			{
				return;
			}
			
			onParsed?.Invoke(configPages);
			progress.Invoke(new ParseProgress(1, string.Empty));
			
		}, token);
	}

	private IConfigPage ParsePlayerConfig(PlayerRemoteConfigPage playerRemoteConfigPage)
	{
		var moveSpeedByLevels = playerRemoteConfigPage.MoveSpeedByLevels;
		var moveSpeedByLevelLocal = new Dictionary<int, float>(moveSpeedByLevels.Count);
		foreach (var moveSpeedByLevel in moveSpeedByLevels)
		{
			moveSpeedByLevelLocal[moveSpeedByLevel.Level] = moveSpeedByLevel.MoveSpeed;
		}

		return new PlayerConfigPage(moveSpeedByLevelLocal, playerRemoteConfigPage.RotationSpeed);
	}

	private IConfigPage ParseDayNightCycleConfig(DayNightCycleRemoteConfigPage dayNightRemoteConfig)
	{
		var lightingByTimeOfDay = new Dictionary<DayPhase, LocalLightingPeriod>();
		var lightingPeriods = new List<LocalLightingPeriod>(dayNightRemoteConfig.LightingPeriods.Count);

		foreach (var remoteLightingPeriod in dayNightRemoteConfig.LightingPeriods)
		{
			var localLightingPeriod = new LocalLightingPeriod(
				(DayPhase)remoteLightingPeriod.TimeOfDay,
				remoteLightingPeriod.Temperature,
				remoteLightingPeriod.Filter,
				remoteLightingPeriod.Intensity,
				remoteLightingPeriod.NormalizedTimeStart,
				remoteLightingPeriod.NormalizedTimeEnd
			);

			lightingByTimeOfDay[localLightingPeriod.DayPhase] = localLightingPeriod;
			lightingPeriods.Add(localLightingPeriod);
		}

		return new DayNightCycleConfigPage(
			dayNightRemoteConfig.DayDuration.TotalMilliseconds,
			dayNightRemoteConfig.NightDuration.TotalMilliseconds,
			lightingByTimeOfDay,
			lightingPeriods,
			dayNightRemoteConfig.AutoStart
		);
	}
}
}