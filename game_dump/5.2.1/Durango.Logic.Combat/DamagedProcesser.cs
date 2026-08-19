using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Player.Animation;
using Durango.Utils;
using Messages;
using Shared.Battle;
using UnityEngine;

namespace Durango.Logic.Combat;

public class DamagedProcesser
{
	private readonly LinkedList<Damaged> _list = new LinkedList<Damaged>();

	public float? ControllLostUntil { get; private set; }

	public event Action<Damaged> Damaged;

	public event Action<float> ControllLost;

	public void Add(Damaged damaged)
	{
		DamageableEntity damageableEntity = GameSystem<CombatSystem>.Instance().DamageableEntities.Get(damaged.AttackerId);
		if (damageableEntity != null)
		{
			damageableEntity.SetPreDamaged(damaged);
		}
		_list.AddLast(damaged);
	}

	public void Update()
	{
		ProcessDamageQueue();
		ProcessBlowTime();
	}

	private void Blow(DamageableEntity attacker)
	{
		if (ControllLostUntil.HasValue)
		{
			return;
		}
		PlayerAnimationClipInfo playerAnimationClipInfo = Singleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationClipInfo("Barehand_DamageBlow");
		if (playerAnimationClipInfo != null)
		{
			if (attacker != null)
			{
				Singleton<PlayerController>.Instance().RotateToPosition(attacker.GetCurrentPosition(), snap: true);
			}
			ControllLostUntil = Time.time + playerAnimationClipInfo.Length;
			Singleton<PlayerController>.Instance().StopMove();
			PlayerController.MotionUpdater.Motion(playerAnimationClipInfo.Clip);
			if (this.ControllLost != null)
			{
				this.ControllLost(ControllLostUntil.Value);
			}
		}
	}

	private void KnockBack(DamageDirection direction)
	{
		if (ControllLostUntil.HasValue)
		{
			return;
		}
		string text = null;
		switch (direction)
		{
		case DamageDirection.Front:
			text = "S";
			break;
		case DamageDirection.Back:
			text = "N";
			break;
		case DamageDirection.Left:
			text = "E";
			break;
		case DamageDirection.Right:
			text = "W";
			break;
		}
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		string text2;
		switch (PlayerBehavior.LocalPlayer.CurrentWeaponFramework)
		{
		case PlayerBehavior.WeaponFramework.TWOHAND:
		case PlayerBehavior.WeaponFramework.LANCE:
			text2 = "Twohand_Damage";
			break;
		case PlayerBehavior.WeaponFramework.CHAINSAW:
			text2 = "Chainsaw_Damage";
			break;
		default:
			text2 = "Onehand_Damage";
			break;
		}
		if (string.IsNullOrEmpty(text2))
		{
			return;
		}
		PlayerAnimationClipInfo playerAnimationClipInfo = Singleton<PlayerAnimationClipManager>.Instance().GetPlayerAnimationClipInfo(text2 + "_" + text);
		if (playerAnimationClipInfo != null)
		{
			ControllLostUntil = Time.time + playerAnimationClipInfo.Length;
			Singleton<PlayerController>.Instance().StopMove();
			PlayerController.MotionUpdater.Motion(playerAnimationClipInfo.Clip);
			if (this.ControllLost != null)
			{
				this.ControllLost(ControllLostUntil.Value);
			}
		}
	}

	private void ProcessBlowTime()
	{
		if (ControllLostUntil.HasValue && !(Time.time < ControllLostUntil.Value))
		{
			ControllLostUntil = null;
		}
	}

	private void ProcessDamageQueue()
	{
		if (_list.Count == 0)
		{
			return;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
		LinkedListNode<Damaged> linkedListNode = _list.First;
		string entityId = PlayerBehavior.LocalPlayer.EntityId;
		while (linkedListNode != null)
		{
			LinkedListNode<Damaged> next = linkedListNode.Next;
			double num = ((!(linkedListNode.Value.AttackerId == entityId)) ? bufferedServerTime : predictedServerTime);
			if (linkedListNode.Value.EventAt < num)
			{
				Damaged value = linkedListNode.Value;
				_list.Remove(linkedListNode);
				Process(value);
			}
			linkedListNode = next;
		}
	}

	private void Process(Damaged damaged)
	{
		DamageableEntities damageableEntities = GameSystem<CombatSystem>.Instance().DamageableEntities;
		DamageableEntity attacker = damageableEntities.Get(damaged.AttackerId);
		DamageableEntity damageableEntity = damageableEntities.Get(damaged.VictimId);
		bool num = damaged.VictimId == PlayerBehavior.LocalPlayer.EntityId;
		Damage damage = damaged.Damage;
		if (damageableEntity != null)
		{
			damageableEntity.OnTakeDamage(damage, attacker);
		}
		if (num)
		{
			if ((damage.Effects & DamageEffects.Blow) != 0)
			{
				Blow(attacker);
			}
			else if ((damage.Effects & DamageEffects.KnockBack) != 0)
			{
				KnockBack(damage.Direction);
			}
		}
		if (this.Damaged != null)
		{
			this.Damaged(damaged);
		}
	}
}
