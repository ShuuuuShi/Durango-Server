using System;
using UnityEngine;

public class TimerBarEffect : MonoBehaviour
{
	[SerializeField]
	private UISprite[] _bars;

	[SerializeField]
	private Color _color;

	private TweenScale[] _tweeners;

	private int _totalLength;

	private void Awake()
	{
		CombatSystem combatSystem = GameSystem<CombatSystem>.Instance();
		combatSystem.ChangedCombatMode += CombatSystem_ChangedCombatMode;
		combatSystem.LeavingBattleStarted = (Action<float>)Delegate.Combine(combatSystem.LeavingBattleStarted, new Action<float>(CombatSystem_LeavingBattleStarted));
		combatSystem.ServerSideBattleBegun = (Action)Delegate.Combine(combatSystem.ServerSideBattleBegun, new Action(CombatSystem_ServerSideBattleBegun));
		InitializeBars();
	}

	private void InitializeBars()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		_tweeners = new TweenScale[_bars.Length];
		_totalLength = 0;
		for (int i = 0; i < _bars.Length; i++)
		{
			UISprite uISprite = _bars[i];
			uISprite.color = _color;
			_tweeners[i] = ((Component)uISprite).gameObject.GetComponent<TweenScale>();
			_totalLength += Mathf.Max(uISprite.width, uISprite.height);
		}
	}

	private void StartEffect(float duration)
	{
		float num = 0f;
		for (int i = 0; i < _tweeners.Length; i++)
		{
			UISprite uISprite = _bars[i];
			TweenScale tweenScale = _tweeners[i];
			((Component)tweenScale).gameObject.SetActive(true);
			tweenScale.ResetToBeginning();
			tweenScale.tweenFactor = 0f;
			tweenScale.duration = duration * ((float)Mathf.Max(uISprite.width, uISprite.height) / (float)_totalLength);
			tweenScale.delay = num;
			tweenScale.PlayForward();
			num += tweenScale.duration;
		}
	}

	private void StopEffect()
	{
		for (int i = 0; i < _tweeners.Length; i++)
		{
			TweenScale tweenScale = _tweeners[i];
			((Component)tweenScale).gameObject.SetActive(false);
		}
	}

	private void CombatSystem_ChangedCombatMode(bool combatMode)
	{
		StopEffect();
	}

	private void CombatSystem_LeavingBattleStarted(float remainTime)
	{
		StartEffect(remainTime);
	}

	private void CombatSystem_ServerSideBattleBegun()
	{
		StopEffect();
	}

	private void OnPortraitMode(bool isPortrait)
	{
		for (int i = 0; i < _bars.Length; i++)
		{
			UISprite uISprite = _bars[i];
			uISprite.ResetAnchors();
		}
	}
}
