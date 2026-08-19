using System.Collections.Generic;
using EnvironmentData;
using FatigueData;
using StatusEffectData;
using UnityEngine;

public class CharacterSurvivalGaugePage : MonoBehaviour
{
	[SerializeField]
	private FatigueWidget _fatigueWidget;

	[SerializeField]
	private ListObjectPool _fatigueMomentums;

	[SerializeField]
	private TweenerPlayer _showAnimation;

	private bool _isPlayShowAnimation;

	private void OnEnable()
	{
		_showAnimation.ResetToBeginning();
		GameSystem<FatigueSystem>.Instance().FatigueUpdated += OnUpdateFatigue;
		OnUpdateFatigue();
	}

	private void OnDisable()
	{
		_isPlayShowAnimation = false;
		GameSystem<FatigueSystem>.Instance().FatigueUpdated -= OnUpdateFatigue;
	}

	private void OnUpdateFatigue()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Fatigue fatigue = GameSystem<FatigueSystem>.Instance().Fatigue;
		List<FatigueVelocity> fatigueVelocities = GameSystem<FatigueSystem>.Instance().FatigueVelocities;
		_fatigueWidget.Set(fatigue, fatigueVelocities);
		_fatigueMomentums.Set(fatigueVelocities.Count);
		IList<StatusEffect> statusEffects = GameSystem<PlayerStatusEffectSystem>.Instance().StatusEffects;
		for (int i = 0; i < fatigueVelocities.Count; i++)
		{
			FatigueVelocity fatigueVelocity = fatigueVelocities[i];
			FatigueMomentum component = _fatigueMomentums[i].GetComponent<FatigueMomentum>();
			component.Set(fatigueVelocity, statusEffects);
		}
		_fatigueMomentums.Reposition(Vector3.down);
	}

	public void ShowAnimation()
	{
		if (!_isPlayShowAnimation)
		{
			_isPlayShowAnimation = true;
			int i = 0;
			for (int count = _fatigueMomentums.Count; i < count; i++)
			{
				_fatigueMomentums[i].GetComponent<UIWidget>().alpha = 0f;
			}
			_showAnimation.Play(PlayShowMomentumsAnimation);
		}
	}

	private void PlayShowMomentumsAnimation()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int count = _fatigueMomentums.Count; i < count; i++)
		{
			_fatigueMomentums[i].GetComponent<UIRect>().alpha = 0f;
			Vector3 localPosition = _fatigueMomentums[i].transform.localPosition;
			_fatigueMomentums[i].transform.localPosition = localPosition + Vector3.left * 10f;
			TweenPosition tweenPosition = TweenPosition.Begin(_fatigueMomentums[i], 0.2f, localPosition);
			tweenPosition.delay = 0.1f * (float)i;
			TweenAlpha tweenAlpha = TweenAlpha.Begin(_fatigueMomentums[i], 0.2f, 1f);
			tweenAlpha.delay = 0.1f * (float)i;
		}
	}
}
