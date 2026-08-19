using System;
using Shared.Economy;

namespace Durango.UI;

[Serializable]
public struct CurrencyData
{
	public Currency CurrencyType;

	public bool IsSkillPoint;
}
