using Durango.Logic;
using UnityEngine;

namespace Durango.UI;

public class MinimapFatigueWarningWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _fatigueEffectSprite;

	private TweenAlpha[] _fatigueEffectTweens;

	private void Awake()
	{
		GameSystem<FatigueSystem>.Instance().FatigueLevelChanged += OnFatigueLevelChanged;
		_fatigueEffectTweens = _fatigueEffectSprite.GetComponents<TweenAlpha>();
	}

	private void OnFatigueLevelChanged(FatigueSystem.FatigueLevel fatigueLevel)
	{
		_fatigueEffectSprite.gameObject.SetActive(value: false);
		TweenAlpha[] fatigueEffectTweens = _fatigueEffectTweens;
		foreach (TweenAlpha tweenAlpha in fatigueEffectTweens)
		{
			tweenAlpha.enabled = false;
		}
		if (fatigueLevel == FatigueSystem.FatigueLevel.BeforeDanger || fatigueLevel == FatigueSystem.FatigueLevel.Danger)
		{
			_fatigueEffectSprite.gameObject.SetActive(value: true);
			int num = (int)(fatigueLevel - 2);
			_fatigueEffectTweens[num].ResetToBeginning();
			_fatigueEffectTweens[num].PlayForward();
		}
	}
}
