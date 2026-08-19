using UnityEngine;

namespace Durango.UI;

public class SideEffectGroup : UIBase
{
	[SerializeField]
	private HitEffectPanel _hitEffect;

	[SerializeField]
	private DeathEffectControl _deathEffect;

	private void Start()
	{
		PlayerBehavior.LocalPlayer.TakenDamage += delegate
		{
			_hitEffect.Play(0.5f, new Color(1f, 0f, 0f, 0.5f));
		};
		PlayerBehavior.LocalPlayer.Died += delegate(CharacterBehavior behavior, bool fromInit)
		{
			if (!fromInit)
			{
				_deathEffect.onFinishedDeathEffect = delegate
				{
					UIManager.FindScript<InteractionGroup>().ShowPlayerDeadInteractionMenu();
				};
				_deathEffect.Play();
			}
		};
	}
}
