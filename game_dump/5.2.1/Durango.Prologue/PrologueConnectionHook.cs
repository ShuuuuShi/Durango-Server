using System.Linq;
using Durango.Logic.Combat;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Battle;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Prologue;

public class PrologueConnectionHook : MonoBehaviour, IConnectionHook
{
	public bool HookSendingMessage(Connection connection, uint sequence, object msg, bool noReply, uint replyOf)
	{
		if (msg is UseBattleAction)
		{
			MakePlayerDamage((UseBattleAction)msg);
		}
		return true;
	}

	private void Start()
	{
		Connections.Frontend.AddHook(this);
		SendActiveActions();
	}

	private void MakePlayerDamage(UseBattleAction action)
	{
		BattleAction battleAction = GameSystem<CombatSystem>.Instance().GetBattleAction(action.ActionId);
		if (battleAction != null)
		{
			PlayerActionAttackInfo playerActionAttackInfo = ((battleAction.Data.AttackInfo != null) ? battleAction.Data.AttackInfo.FirstOrDefault() : null);
			if (playerActionAttackInfo != null && !(Durango.Utils.Singleton<ObjectManager>.Instance().FindCharacter(action.TargetEntityId) == null))
			{
				Damaged damaged = default(Damaged);
				damaged.AttackerId = GameManager.PlayerId;
				damaged.VictimId = action.TargetEntityId;
				damaged.Damage = new Damage
				{
					Result = DamageResult.Hit,
					AttackType = AttackType.Axe,
					Effects = (DamageEffects.Critical | DamageEffects.Blow),
					Part = BodyPart.Body,
					Direction = DamageDirection.Front,
					Value = Random.Range(30, 50)
				};
				damaged.EventAt = Connections.Frontend.GetBufferedServerTime() + (double)playerActionAttackInfo.AttackTime;
				Damaged msg = damaged;
				Connections.Frontend.PushPacket(msg);
			}
		}
	}

	private void SendActiveActions()
	{
		BattleAction battleAction = new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_dodge"));
		battleAction.Motion = "Novice_Dodge";
		GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[1] { battleAction });
	}
}
