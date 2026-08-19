using System;
using System.Linq;

namespace Durango.Logic.PlayGuide;

internal class StatusEffectCondition : FlowCondition
{
	private readonly string[] _effectIds;

	public StatusEffectCondition(string param)
	{
		if (!string.IsNullOrEmpty(param))
		{
			_effectIds = param.Split('|');
			for (int i = 0; i < _effectIds.Length; i++)
			{
				_effectIds[i] = _effectIds[i].Trim();
			}
		}
	}

	private void OnAddStatusEffect(string entityId, string id)
	{
		if (!(entityId != GameManager.PlayerId))
		{
			if (KUtility.GetSize(_effectIds) == 0)
			{
				Interrupt();
			}
			else if (_effectIds.Any((string t) => string.Compare(t, id, StringComparison.OrdinalIgnoreCase) == 0))
			{
				Interrupt();
			}
		}
	}

	protected override void OnRegister()
	{
		GameSystem<StatusEffectSystem>.Instance().StatusEffectAdded += OnAddStatusEffect;
	}

	protected override void OnUnregister()
	{
		GameSystem<StatusEffectSystem>.Instance().StatusEffectAdded -= OnAddStatusEffect;
	}
}
