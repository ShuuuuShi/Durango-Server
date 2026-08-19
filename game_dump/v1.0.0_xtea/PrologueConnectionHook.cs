using System.Collections.Generic;
using CombatData;
using K1Network;
using L10N;
using Messages;
using MsgPack;
using Shared.Battle;
using UnityEngine;

public class PrologueConnectionHook : ConnectionHook
{
	private PrologueAIPlayer _aiPlayer;

	private void Start()
	{
		Connections.Frontend.SetHook(this);
		_aiPlayer = ((Component)this).GetComponent<PrologueAIPlayer>();
		_aiPlayer.SetConnectionHook(this);
	}

	public override bool HookSendingMessage(object msg)
	{
		if (msg is Equip)
		{
			SendEquipment((Equip)msg);
		}
		if (msg is GetActions)
		{
			SendActiveActions();
		}
		if (msg is ReserveAction)
		{
			BattleReserveAction((ReserveAction)msg);
		}
		if (msg is EnterBattle)
		{
			BattleEntered();
		}
		if (msg is ExitBattle)
		{
			BattleExited();
		}
		return true;
	}

	private void BattleEntered()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			return;
		}
		PrologueInteractionExtension.AdjustActionButtonPosition();
		AnimalBehavior component = KSingleton<ObjectManager>.Instance().FindObject(PrologueAIRaptor.FakeEntityId).GetComponent<AnimalBehavior>();
		if (!Object.op_Implicit((Object)(object)component) || !((Component)component).gameObject.activeInHierarchy)
		{
			return;
		}
		_aiPlayer.BattleEntered(component);
		MessagePackObjectDictionary val = new MessagePackObjectDictionary();
		Notify msg = default(Notify);
		msg.Method = 1502;
		val.Add(MessagePackObject.op_Implicit("entity_id"), MessagePackObject.op_Implicit(PrologueAIRaptor.FakeEntityId));
		val.Add(MessagePackObject.op_Implicit("event_at"), MessagePackObject.op_Implicit(0));
		msg.Data = val;
		DispatchNotifyMsg(msg);
		KSingleton<TrainTrexController>.Instance().OnBeginAutoBattle();
		SendActiveActionsNotify();
		Connections.Frontend.Send(default(GetActions));
		((Component)component).GetComponent<PrologueAIRaptor>().SetAiActivated();
		ActionButtonGroup actionButtonGroup = UIManager.FindScript<ActionButtonGroup>();
		foreach (KeyValuePair<string, PrologueAIPlayer.ActionInfo> actionSet in _aiPlayer.ActionSets)
		{
			bool isAutoAction = actionSet.Value.IsAutoAction;
			actionButtonGroup.ActionButtons.SetActionButtonState(actionSet.Key, (!isAutoAction) ? ActionState.Hidden : ActionState.Activated);
		}
	}

	private void BattleExited()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		_aiPlayer.BattleExited();
		MessagePackObjectDictionary val = new MessagePackObjectDictionary();
		Notify msg = default(Notify);
		msg.Method = 1503;
		val.Add(MessagePackObject.op_Implicit("entity_id"), MessagePackObject.op_Implicit(PrologueAIRaptor.FakeEntityId));
		val.Add(MessagePackObject.op_Implicit("event_at"), MessagePackObject.op_Implicit(0));
		msg.Data = val;
		DispatchNotifyMsg(msg);
	}

	private void BattleReserveAction(ReserveAction msg)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (msg.ActionSet == "move_to")
		{
			_aiPlayer.BattleReserveMove(new Vector3(msg.Pos.x, 0f, msg.Pos.y));
		}
		else
		{
			_aiPlayer.BattleReserveAction(msg.ActionSet);
		}
	}

	public void RequestNotifyActionReserved(string actionSetId, string actionId, EfxType efxType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		MessagePackObjectDictionary val = new MessagePackObjectDictionary();
		Notify msg = default(Notify);
		msg.Method = 1516;
		val.Add(MessagePackObject.op_Implicit("action_set_id"), MessagePackObject.op_Implicit(actionSetId));
		val.Add(MessagePackObject.op_Implicit("action_id"), MessagePackObject.op_Implicit(actionId));
		val.Add(MessagePackObject.op_Implicit("efx_type"), MessagePackObject.op_Implicit(efxType.ToString()));
		val.Add(MessagePackObject.op_Implicit("event_at"), MessagePackObject.op_Implicit(0));
		val.Add(MessagePackObject.op_Implicit("until"), MessagePackObject.op_Implicit(0));
		msg.Data = val;
		DispatchNotifyMsg(msg);
	}

	public void SendActiveActionCanceled()
	{
		ActiveActionCanceled activeActionCanceled = default(ActiveActionCanceled);
		activeActionCanceled.CanceledAt = Connections.Frontend.GetPredictedServerTime();
		Connections.Frontend.Handle(602u, activeActionCanceled, default(PacketHeader));
	}

	public void SendReactiveActionStandBy(string actionSetId, string actionId, double at, double standByTime, double coolDownUntil)
	{
		ReactiveActionStandby reactiveActionStandby = default(ReactiveActionStandby);
		reactiveActionStandby.ActionSetId = actionSetId;
		reactiveActionStandby.ActionId = actionId;
		reactiveActionStandby.Since = at;
		reactiveActionStandby.Until = standByTime;
		reactiveActionStandby.CooldownUntil = coolDownUntil;
		Connections.Frontend.Handle(604u, reactiveActionStandby, default(PacketHeader));
	}

	public void SendActiveActionUsed(string actionSetId, string actionId, float coolDown)
	{
		ActiveActionUsed activeActionUsed = default(ActiveActionUsed);
		activeActionUsed.ActionSetId = actionSetId;
		activeActionUsed.ActionId = actionId;
		activeActionUsed.UsedAt = Connections.Frontend.GetPredictedServerTime();
		activeActionUsed.CooldownUntil = Connections.Frontend.GetPredictedServerTime() + (double)coolDown;
		Connections.Frontend.Handle(603u, activeActionUsed, default(PacketHeader));
	}

	private void SendActiveActionsNotify()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		MessagePackObjectDictionary val = new MessagePackObjectDictionary();
		Notify msg = default(Notify);
		msg.Method = 1510;
		val.Add(MessagePackObject.op_Implicit("event_at"), MessagePackObject.op_Implicit(0));
		List<MessagePackObject> list = new List<MessagePackObject>();
		foreach (KeyValuePair<string, PrologueAIPlayer.ActionInfo> actionSet in _aiPlayer.ActionSets)
		{
			MessagePackObjectDictionary val2 = new MessagePackObjectDictionary();
			val2.Add(MessagePackObject.op_Implicit("id"), MessagePackObject.op_Implicit(actionSet.Key));
			val2.Add(MessagePackObject.op_Implicit("state"), MessagePackObject.op_Implicit(4));
			val2.Add(MessagePackObject.op_Implicit("since"), MessagePackObject.op_Implicit(Connections.Frontend.GetPredictedServerTime()));
			val2.Add(MessagePackObject.op_Implicit("until"), MessagePackObject.op_Implicit(Connections.Frontend.GetPredictedServerTime() + 2.0));
			list.Add(new MessagePackObject(val2));
		}
		val.Add(MessagePackObject.op_Implicit("actions_list"), new MessagePackObject((IList<MessagePackObject>)list));
		msg.Data = val;
		DispatchNotifyMsg(msg);
	}

	public void SendActionBegun(string actionSetId, double until, double cooldownUntil)
	{
		ActionBegun actionBegun = default(ActionBegun);
		actionBegun.Since = Time.time;
		actionBegun.Until = until;
		actionBegun.ActionSetId = actionSetId;
		actionBegun.CooldownUntil = cooldownUntil;
		Connections.Frontend.Handle(609u, actionBegun, default(PacketHeader));
	}

	private void SendEquipment(Equip msg)
	{
		Equipments equipments = default(Equipments);
		equipments.Slots = new Dictionary<string, Item>();
		Item value = default(Item);
		value.Id = 0uL;
		value.Name = T._("뾰족한 물체");
		value.Icon = "weapon_axe_onehand_metal_3";
		value.Size = 0;
		value.Description = T._("뾰족한 물체");
		value.Prototype = string.Empty;
		List<Performance> list = new List<Performance>();
		Performance item = default(Performance);
		item.Id = "weapon";
		item.Icon = "tag_purpose_weapon";
		item.Name = T._("무기");
		item.Nums = new Dictionary<string, float>();
		item.Strs = new Dictionary<string, string>();
		item.Nums.Add("damages.pierce", 108f);
		item.Nums.Add("damages.impact", 0f);
		item.Nums.Add("damages.cut", 0f);
		item.Nums.Add("range", 600f);
		item.Strs.Add("slot", "both");
		item.Nums.Add("accuracy", 120f);
		item.Strs.Add("accuracy_type", "melee_normal");
		item.Nums.Add("critical", 120f);
		item.Nums.Add("stamina_cost", 4f);
		item.Nums.Add("attack_cooltime", 1f);
		item.Strs.Add("attack_type", "sword");
		item.Strs.Add("weapon_framework", "prologue_weapon");
		int num = 0;
		foreach (KeyValuePair<string, PrologueAIPlayer.ActionInfo> actionSet in _aiPlayer.ActionSets)
		{
			item.Strs.Add($"action{num}", actionSet.Key);
			num++;
		}
		list.Add(item);
		Performance item2 = default(Performance);
		item2.Id = "melee_weapon";
		item2.Icon = "tag_purpose_weapon";
		item2.Name = T._("무기");
		list.Add(item2);
		Performance item3 = default(Performance);
		item3.Id = "purpose";
		item3.Icon = "icon_tag_purpose";
		item3.Name = T._("용도");
		list.Add(item3);
		value.Performance = list.ToArray();
		value.Tags = new Tag[0];
		value.TagModifications = new Tag[0];
		equipments.Slots.Add(msg.SlotName, value);
		Connections.Frontend.Handle(111u, equipments, default(PacketHeader));
	}

	private void SendActiveActions()
	{
		Actions actions = default(Actions);
		actions.ActionSetAvailabilities = new Dictionary<string, bool>();
		foreach (KeyValuePair<string, PrologueAIPlayer.ActionInfo> actionSet in _aiPlayer.ActionSets)
		{
			actions.ActionSetAvailabilities.Add(actionSet.Key, value: true);
		}
		Connections.Frontend.Handle(315u, actions, default(PacketHeader));
	}

	private void DispatchNotifyMsg(Notify msg)
	{
		Connections.Frontend.GetNotificationHandler(msg.Method)?.Invoke(msg, default(PacketHeader));
	}

	public void RequestNotifyMsg(Notify msg)
	{
		DispatchNotifyMsg(msg);
	}
}
