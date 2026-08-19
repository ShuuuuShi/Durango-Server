using Messages;
using Shared.Battle;
using UnityEngine;

namespace PlayerExtensionsPrologue;

internal static class PlayerBehaviorExtension
{
	public static void MakePrologueMode(this PlayerBehavior player)
	{
		player.AttackSucceeded += player.LocalPlayer_AttackSucceeded;
		player.Life = new Gauge(100f, 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = 100f
			}
		});
		player.IgnoreOcclusionCheck = true;
		player.IsOutlineEnabled = false;
		player.Started += delegate
		{
			player.PlayAnimation("Stand");
			player.LateMotionUpdate();
			player.RootMotionMovable.SetLocalRootMotionYawMode(isIgnoreYaw: true);
			KSingleton<UIManager>.Instance().PlayerFloatingGroup.HideLocalPlayer();
		};
		player.ChangeWeaponType(PlayerBehavior.WeaponFramework.BAREHAND);
		player.IsPlaneShadowEnabled = false;
		KSingleton<ContactShadowManager>.Instance().Create(((Component)player).gameObject, isRapidUpdateMode: true, destroyIfInvisible: false);
	}

	public static void UndoPrologueMode(this PlayerBehavior player)
	{
		player.AttackSucceeded -= player.LocalPlayer_AttackSucceeded;
	}

	public static void AddClip(this PlayerBehavior player, AnimationClip clip)
	{
		if ((Object)null == (Object)(object)player.Anim.GetClip(((Object)clip).name))
		{
			player.Anim.AddClip(clip, ((Object)clip).name);
		}
	}

	public static void PlayClip(this PlayerBehavior player, AnimationClip clip)
	{
		if ((Object)null == (Object)(object)player.Anim.GetClip(((Object)clip).name))
		{
			player.Anim.AddClip(clip, ((Object)clip).name);
		}
		player.Anim.CrossFade(((Object)clip).name, 0.1f);
	}

	public static float GetClipLength(this PlayerBehavior player, string clipKeyName)
	{
		string text = string.Format("{0}_{1}", (!player.IsMale) ? "F" : "M", clipKeyName);
		AnimationClip clip = player.Anim.GetClip(text);
		if ((Object)(object)clip == (Object)null)
		{
			return 0f;
		}
		return clip.length;
	}

	public static void ReceiveDamage(this PlayerBehavior player, GameObject attacker, Damage damage)
	{
		if ((Object)(object)player != (Object)null)
		{
			PrologueManager.PlayerBattleAi.ReceiveDamage(attacker, damage);
			player.OnTakeDamage(damage, attacker);
		}
	}

	private static void LocalPlayer_AttackSucceeded(this PlayerBehavior player, CharacterBehavior character, BodyPart part, string animKeyName)
	{
		PrologueManager.PlayerBattleAi.MakeDamageToDino(character, part, animKeyName);
	}
}
