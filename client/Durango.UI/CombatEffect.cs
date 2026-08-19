using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class CombatEffect : AnimationWidget
{
	[SerializeField]
	private CombatStateEffect _stateEffect;

	private bool _showEffect;

	private void Start()
	{
		base.gameObject.SetActive(value: false);
		base.Widget.alpha = 0f;
		if (GameManager.IsPrologueMode)
		{
			return;
		}
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += delegate(bool isCombat)
		{
			UpdateState();
			if (isCombat)
			{
				_stateEffect.ResetTimer();
			}
		};
		UIManager.FindScript<CombatGroup>().BattleViewChanged += delegate
		{
			UpdateState();
		};
	}

	private void UpdateState()
	{
		CombatGroup.BattleViewMode battleView = UIManager.FindScript<CombatGroup>().BattleView;
		bool flag;
		if (battleView == CombatGroup.BattleViewMode.Mount)
		{
			flag = true;
			_stateEffect.SetColor(new Color32(145, 27, 27, byte.MaxValue));
		}
		else if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			flag = true;
			_stateEffect.SetColor(new Color32(145, 27, 27, byte.MaxValue));
		}
		else if (battleView == CombatGroup.BattleViewMode.Battle)
		{
			flag = true;
			_stateEffect.SetColor(PresetColor.UIYellow);
		}
		else
		{
			flag = false;
		}
		if (_showEffect != flag)
		{
			if (flag)
			{
				base.gameObject.SetActive(value: true);
				SetAlpha(1f);
			}
			else
			{
				SetAlpha(0f);
			}
			_showEffect = flag;
		}
	}
}
