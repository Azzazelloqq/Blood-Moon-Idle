using System;
using Runtime.Core.Architecture.CompositionRoot.Base;

namespace Runtime.Core.Architecture.CompositionRoot.Factory
{
public interface ICompositionRootFactory
{
	public ICompositionRoot Get(Type rootType);
}
}