using System;
using System.Collections;
using System.Collections.Generic;
using K1Network;
using Messages;
using Shared.Animal;
using Shared.Battle;
using UnityEngine;

public class ObjectManager : KSingleton<ObjectManager>
{
	private AnimalManager _animalMgr;

	private PlayerManager _playerMgr;

	public static float GetDistanceWithTargetRadius(GameObject my, GameObject target)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		CharacterBehavior component = my.GetComponent<CharacterBehavior>();
		CharacterBehavior component2 = target.GetComponent<CharacterBehavior>();
		if ((Object)(object)component2 == (Object)null || (Object)(object)component == (Object)null)
		{
			Vector3 val = my.transform.localPosition - target.transform.localPosition;
			return ((Vector3)(ref val)).magnitude;
		}
		Vector3 val2 = component2.CurrentPosition - component.CurrentPosition;
		float num = Mathf.Atan2(val2.x, val2.z);
		float magnitude = ((Vector3)(ref val2)).magnitude;
		float currentYaw = component2.CurrentYaw;
		float angle = Mathf.Abs(num - currentYaw);
		float targetRadius = GetTargetRadius(target, angle);
		return magnitude - targetRadius;
	}

	public static float GetTargetRadius(GameObject target)
	{
		CharacterBehavior component = target.GetComponent<CharacterBehavior>();
		if ((Object)(object)component == (Object)null)
		{
			return 0f;
		}
		return (component.XRadius + component.YRadius) / 2f;
	}

	public static float GetTargetRadius(GameObject target, float angle)
	{
		CharacterBehavior component = target.GetComponent<CharacterBehavior>();
		if ((Object)(object)component == (Object)null)
		{
			return 0f;
		}
		angle *= (float)Math.PI / 180f;
		return Mathf.Sqrt(Mathf.Pow(component.XRadius * Mathf.Sin(angle), 2f) + Mathf.Pow(component.YRadius * Mathf.Cos(angle), 2f));
	}

	protected override void OnAwake()
	{
		_animalMgr = KSingleton<AnimalManager>.Instance();
		_playerMgr = KSingleton<PlayerManager>.Instance();
	}

	private void Start()
	{
		Connections.Frontend.On(delegate(Move msg, PacketHeader header)
		{
			if (!_playerMgr.HandleMoveMsg(msg))
			{
				_animalMgr.HandleMoveMsg(msg);
			}
		});
		Connections.Frontend.On(delegate(DisappearEntity msg, PacketHeader header)
		{
			if (!_playerMgr.HandleDisappearMsg(msg) && !_animalMgr.HandleDisappearMsg(msg) && !KSingleton<StaticObjectManager>.Instance().HandleDisappearMsg(msg))
			{
			}
		});
		Connections.Frontend.On(delegate(Messages.Survival msg, PacketHeader header)
		{
			float timeDelay = 0f;
			if (header.Time > Connections.Frontend.GetBufferedServerTime_Enhanced())
			{
				timeDelay = (float)(header.Time - Connections.Frontend.GetBufferedServerTime_Enhanced());
			}
			CharacterBehavior characterBehavior = FindCharacter(msg.EntityId);
			if ((Object)(object)characterBehavior != (Object)null)
			{
				((MonoBehaviour)this).StartCoroutine(ApplySurvivalGauge(characterBehavior, msg.Life, msg.Gauges, timeDelay));
			}
		});
		Connections.Frontend.On(delegate(Damaged msg, PacketHeader header)
		{
			float num = (float)(msg.EventAt - Connections.Frontend.GetBufferedServerTime_Enhanced());
			ulong victimId = msg.VictimId;
			GameObject attacker = FindObject(msg.AttackerId);
			if (!CombatSystem.EnableDamageLog || msg.AttackerId == PlayerBehavior.LocalPlayer.EntityId)
			{
			}
			((MonoBehaviour)this).StartCoroutine(MakeDamage(victimId, msg.Damage, attacker, msg.EventAt, msg.AttackerId == PlayerBehavior.LocalPlayer.EntityId));
		});
		Connections.Frontend.On<BattleLog>(delegate
		{
		});
		Connections.Frontend.On<LivingLog>(delegate
		{
		});
		Connections.Frontend.On(delegate(CombatInteraction msg, PacketHeader header)
		{
			if (msg.Details.ContainsKey("look_at"))
			{
				GameObject val = FindObject(msg.EntityId);
				if (Object.op_Implicit((Object)(object)val))
				{
					BoneLookAtTarget component = val.GetComponent<BoneLookAtTarget>();
					if ((Object)(object)component != (Object)null)
					{
						GameObject target = null;
						if (msg.TargetId != 0L)
						{
							target = FindObject(msg.TargetId);
						}
						component.SetLookTarget(target, bFindHead: true);
					}
				}
			}
			if (msg.Details.ContainsKey("joined"))
			{
			}
			if (msg.Details.ContainsKey("disjoined"))
			{
			}
			if (msg.Details.ContainsKey("hold"))
			{
				PlayerBehavior player = KSingleton<PlayerManager>.Instance().GetPlayer(msg.EntityId);
				if ((Object)(object)player != (Object)null)
				{
					if (msg.Details["hold"] == 0L)
					{
						player.RestoreStandState();
					}
					else
					{
						player.SetStandState(PlayerBehavior.StandStateEnum.HoldMode);
					}
				}
			}
			if (msg.Details.ContainsKey("status"))
			{
				AnimalStatus status = (AnimalStatus)msg.Details["status"];
				AnimalBehavior animal = KSingleton<AnimalManager>.Instance().GetAnimal(msg.EntityId);
				if ((Object)(object)animal != (Object)null)
				{
					float delay2 = (float)(header.Time - Connections.Frontend.GetBufferedServerTime());
					((MonoBehaviour)this).StartCoroutine(CoDelayAnimalStatusMsg(delay2, animal, status));
				}
			}
			if (msg.Details.ContainsKey("notice_attack"))
			{
				AnimalBehavior animal2 = KSingleton<AnimalManager>.Instance().GetAnimal(msg.EntityId);
				if ((Object)(object)animal2 != (Object)null)
				{
					animal2.AttackNotice((double)msg.Details["notice_attack"] / 1000.0);
				}
			}
		});
		Connections.Frontend.On(delegate(StatusEffects msg, PacketHeader header)
		{
			if (PlayerBehavior.LocalPlayer.EntityId == msg.EntityId)
			{
				GameSystem<PlayerStatusEffectSystem>.Instance().SetStatusEffects(msg._StatusEffects);
			}
			else
			{
				float delay = (float)(header.Time - Connections.Frontend.GetBufferedServerTime_Enhanced());
				KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
				{
					GameSystem<TargetStatusEffectSystem>.Instance().SetStatusEffects(msg._StatusEffects);
				}, delay);
			}
		});
	}

	private static IEnumerator CoDelayAnimalStatusMsg(float delay, AnimalBehavior animal, AnimalStatus status)
	{
		yield return (object)new WaitForSeconds(delay);
		animal.Status = status;
	}

	private IEnumerator MakeDamage(ulong pid, Damage damage, GameObject attacker, double eventAt, bool attackerIsLocalPlayer)
	{
		if ((Object)(object)attacker == (Object)null)
		{
			yield break;
		}
		PlayerBehavior playerAttacker = attacker.GetComponent<PlayerBehavior>();
		if (Object.op_Implicit((Object)(object)playerAttacker))
		{
			playerAttacker.ProjectileController.LastAttackBodyPart = damage.Part;
		}
		float timeDelay = (float)(eventAt - Connections.Frontend.GetBufferedServerTime_Enhanced());
		Artifact artifactAttacker = attacker.GetComponent<Artifact>();
		if (Object.op_Implicit((Object)(object)artifactAttacker))
		{
			Defensive defensive = artifactAttacker.GetArtifactComponent<Defensive>();
			if (defensive != null)
			{
				GameObject victim = FindObject(pid);
				defensive.ShootProjectile(victim, damage.Part, timeDelay);
			}
		}
		yield return (object)new WaitForSeconds(timeDelay);
		if (attackerIsLocalPlayer)
		{
			float expireTime = Time.time + PlayerBehavior.LocalPlayer.AttackFrameExpireTime;
			while (!PlayerBehavior.LocalPlayer.IsAttackFramePassed && !(Time.time > expireTime))
			{
				yield return null;
			}
			if (CombatSystem.EnableDamageLog)
			{
			}
			PlayerBehavior.LocalPlayer.IsAttackFramePassed = false;
			PlayerBehavior.LocalPlayer.DamageResultReceived(damage);
		}
		CharacterBehavior victimAnimal = FindCharacter(pid);
		if ((Object)(object)victimAnimal != (Object)null)
		{
			victimAnimal.OnTakeDamage(damage, attacker);
			CharacterBehavior attackerCharacter = null;
			if (Object.op_Implicit((Object)(object)attacker))
			{
				attackerCharacter = attacker.GetComponent<CharacterBehavior>();
			}
			if (damage.Value > 0 && attackerIsLocalPlayer && (damage.Effects & DamageEffects.Critical) != 0)
			{
				KSingleton<CameraShaker>.Instance().DamageShake(Mathf.Max(damage.Value, 30));
			}
			UIManager.AddDamageLabel(victimAnimal, damage, attackerCharacter);
		}
		Artifact artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(pid);
		if ((Object)(object)artifact != (Object)null)
		{
			artifact.OnTakeDamage(damage, attacker);
		}
	}

	private IEnumerator ApplySurvivalGauge(CharacterBehavior character, Gauge life, Dictionary<string, Gauge> gauges, float timeDelay)
	{
		yield return (object)new WaitForSeconds(timeDelay);
		if ((Object)(object)character != (Object)null)
		{
			character.SetSurvivalGauge(life, gauges);
		}
	}

	public GameObject FindObject(ulong id)
	{
		CharacterBehavior characterBehavior = FindCharacter(id);
		if ((Object)(object)characterBehavior != (Object)null)
		{
			return ((Component)characterBehavior).gameObject;
		}
		Artifact artifact = KSingleton<StaticObjectManager>.Instance().FindArtifact(id);
		if ((Object)(object)artifact != (Object)null)
		{
			return ((Component)artifact).gameObject;
		}
		return null;
	}

	public CharacterBehavior FindCharacter(ulong id)
	{
		CharacterBehavior playerIncludeLocalPlayer = _playerMgr.GetPlayerIncludeLocalPlayer(id);
		return (!((Object)(object)playerIncludeLocalPlayer != (Object)null)) ? _animalMgr.GetAnimal(id) : playerIncludeLocalPlayer;
	}

	public void ForceAddAnimal(ulong id, AnimalBehavior animal)
	{
		_animalMgr.ForceAddAnimal(id, animal);
	}
}
