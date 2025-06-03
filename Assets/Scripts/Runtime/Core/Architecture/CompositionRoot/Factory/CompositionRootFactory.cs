using System;
using Runtime.Core.Architecture.CompositionRoot.Base;
using Runtime.Core.Architecture.CompositionRoot.Gameplay;
using Runtime.Core.Architecture.CompositionRoot.Gameplay.City;
using Runtime.Core.Architecture.CompositionRoot.Gameplay.Crypt;
using Runtime.Core.Architecture.CompositionRoot.Main;

namespace Runtime.Core.Architecture.CompositionRoot.Factory
{
internal class CompositionRootFactory : ICompositionRootFactory
{
	public ICompositionRoot Get(Type rootType)
	{
		if (rootType == typeof(GameCompositionRoot))
		{
			return new GameCompositionRoot();
		}

		if (rootType == typeof(GameplayCompositionRoot))
		{
			return GameplayCompositionRootFactory.CreateGameplayCompositionRoot();
		}

		if (rootType == typeof(CityComposition))
		{
			return CityCompositionFactory.CreateCityComposition();
		}

		if (rootType == typeof(CryptComposition))
		{
			return CryptCompositionFactory.CreateCryptComposition();
		}

		throw new NotSupportedException(rootType.FullName);
	}
}
}