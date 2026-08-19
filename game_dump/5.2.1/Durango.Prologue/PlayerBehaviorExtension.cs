using Durango.Render;
using Durango.UI;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public static class PlayerBehaviorExtension
{
	public static void MakePrologueMode(this PlayerBehavior player)
	{
		player.Life = new Gauge(100f, 0f, new GaugeNode[1]
		{
			new GaugeNode
			{
				Time = 0.0,
				Value = 100f
			}
		});
		player.OutlineEnabled = false;
		player.Started += delegate
		{
			player.PlayMotionForcely("Barehand_Stand", 1f, immediately: true);
			player.RootMotionMovable.SetLocalRootMotionYawMode(isIgnoreYaw: true);
			UIManager.FindScript<PlayerFloatingGroup>().HideLocalPlayer();
		};
		player.ChangeWeaponType(PlayerBehavior.WeaponFramework.BAREHAND);
		player.PlaneShadowEnabled = false;
		Singleton<ContactShadowManager>.Instance().Create(player.gameObject, isRapidUpdateMode: true, destroyIfInvisible: false);
	}

	public static void AddClip(this PlayerBehavior player, AnimationClip clip)
	{
		if (null == player.Anim.GetClip(clip.name))
		{
			player.Anim.AddClip(clip, clip.name);
		}
	}

	public static void PlayClip(this PlayerBehavior player, AnimationClip clip)
	{
		if (null == player.Anim.GetClip(clip.name))
		{
			player.Anim.AddClip(clip, clip.name);
		}
		player.Anim.CrossFade(clip.name, 0.1f);
	}
}
