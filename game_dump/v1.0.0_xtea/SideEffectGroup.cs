using System;
using UnityEngine;

public class SideEffectGroup : UIBase
{
	[SerializeField]
	private HitEffectPanel _hitEffect;

	[SerializeField]
	private DeathEffectControl _deathEffect;

	[SerializeField]
	private CombatEffect _combatEffect;

	[SerializeField]
	private CenterEffectControl _centerEffect;

	public CenterEffectControl CenterEffect => _centerEffect;

	public void StartSideEffect(Color color, float duration = 0.5f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_hitEffect.color = color;
		_hitEffect.duration = duration;
		_hitEffect.StartHitEffect();
	}

	public void PlayDeathEffect(string deathMsg, Action finishFunc)
	{
		_deathEffect.onFinishedDeathEffect = finishFunc;
		_deathEffect.SetDescription(deathMsg);
		_deathEffect.Play();
	}

	public void SetCombatEffect(CombatSystem.State state)
	{
		_combatEffect.SetCombatEffect(state);
	}

	private void Awake()
	{
		((Component)_hitEffect).gameObject.SetActive(true);
	}
}
