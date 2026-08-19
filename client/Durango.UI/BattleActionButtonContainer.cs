using System;
using Durango.Logic.Combat;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class BattleActionButtonContainer : MonoBehaviour
{
	[Serializable]
	private class SlotArgument
	{
		public Vector2 Position;

		public float Scale;

		public float ShowDelay;

		[NonSerialized]
		public BattleActionButton Button;
	}

	[SerializeField]
	private BattleActionButton _actionButtonBase;

	[SerializeField]
	private SlotArgument[] _playerActionSlots;

	[SerializeField]
	private SlotArgument _petActionSlot;

	[SerializeField]
	private SlotArgument _tameActinoSlot;

	public int PlayerActionSlotCount { get; private set; }

	public event Action<BattleActionButton, bool> PlayerActionPressed;

	public event Action<BattleActionButton, bool> PetActionPressed;

	public event Action<BattleActionButton, bool> TameActionPressed;

	private void Awake()
	{
		_actionButtonBase.gameObject.SetActive(value: false);
		PlayerActionSlotCount = _playerActionSlots.Length;
		Action<BattleActionButton, bool> pressed = delegate(BattleActionButton btn, bool press)
		{
			if (this.PlayerActionPressed != null)
			{
				this.PlayerActionPressed(btn, press);
			}
		};
		SlotArgument[] playerActionSlots = _playerActionSlots;
		foreach (SlotArgument args in playerActionSlots)
		{
			InitializeButton(args, pressed);
		}
		InitializeButton(_petActionSlot, delegate(BattleActionButton btn, bool press)
		{
			if (this.PetActionPressed != null)
			{
				this.PetActionPressed(btn, press);
			}
		});
		InitializeButton(_tameActinoSlot, delegate(BattleActionButton btn, bool press)
		{
			if (this.TameActionPressed != null)
			{
				this.TameActionPressed(btn, press);
			}
		});
	}

	private void InitializeButton(SlotArgument args, Action<BattleActionButton, bool> pressed)
	{
		GameObject obj = base.gameObject.AddChild(_actionButtonBase.gameObject);
		Transform obj2 = obj.transform;
		obj2.localPosition = args.Position;
		obj2.localScale = Vector3.one * args.Scale;
		BattleActionButton component = obj.GetComponent<BattleActionButton>();
		component.Pressed += pressed;
		component.ShowDelay = args.ShowDelay;
		component.SetEmpty();
		args.Button = component;
	}

	public void SetAction(int index, [CanBeNull] BattleAction action)
	{
		if (action == null)
		{
			_playerActionSlots[index].Button.SetEmpty();
			return;
		}
		double cooldownSince = action.CooldownSince;
		double cooldownUntil = action.CooldownUntil;
		BattleActionButton button = _playerActionSlots[index].Button;
		button.SetIcon(action.Data.Meta.Icon);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		BattleActionButton.ButtonState state = ((cooldownSince <= predictedServerTime && predictedServerTime < cooldownUntil) ? BattleActionButton.ButtonState.Cooldown : (((TerrainWater.WaterDepthLevel)PlayerBehavior.LocalPlayer.WaterDepthLevel >= TerrainWater.WaterDepthLevel.Waist) ? BattleActionButton.ButtonState.Unusable : ((GameSystem<CombatSystem>.Instance().DamagedProcesser.ControllLostUntil.GetValueOrDefault() > Time.time) ? BattleActionButton.ButtonState.Unusable : ((predictedServerTime < action.ProhibitedUntil) ? BattleActionButton.ButtonState.Prohibited : BattleActionButton.ButtonState.Activate))));
		button.SetTimer(cooldownSince, cooldownUntil);
		button.Set(action.Data.Id, state);
	}

	public void SetPlaceholderAction(int index, [CanBeNull] PlayerAction action)
	{
		BattleActionButton button = _playerActionSlots[index].Button;
		button.SetIcon(action?.Meta.Icon);
		button.SetTimer(0.0, 0.0);
		button.Set(action?.Id, BattleActionButton.ButtonState.Locked);
	}

	public void SetPetAction(Messages.PetActiveSkill? action)
	{
		Yaml.PetActiveSkill petActiveSkill = (action.HasValue ? PetActiveSkills.Get(action.Value.SkillId, action.Value.Rank) : null);
		if (petActiveSkill == null)
		{
			_petActionSlot.Button.SetEmpty();
			return;
		}
		PetSkillStates.State state = Durango.Utils.Singleton<PetManager>.Instance().PlayerPetSkillStates.GetState(action.Value.SkillId);
		if (state == null)
		{
			_petActionSlot.Button.SetEmpty();
			return;
		}
		BattleActionButton button = _petActionSlot.Button;
		button.SetIcon(petActiveSkill.Icon, PresetColor.UIPet);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double? reactivatedAt = state.ReactivatedAt;
		double num;
		double num2;
		if (!reactivatedAt.HasValue || state.ReactivatedAt.Value < predictedServerTime)
		{
			num = 0.0;
			num2 = 0.0;
		}
		else
		{
			num = ((!state.UsedAt.HasValue) ? 0.0 : state.UsedAt.Value);
			num2 = state.ReactivatedAt.Value;
		}
		BattleActionButton.ButtonState state2 = ((num < predictedServerTime && predictedServerTime < num2) ? BattleActionButton.ButtonState.Cooldown : (state.ReservedAt.HasValue ? BattleActionButton.ButtonState.Reserved : BattleActionButton.ButtonState.Activate));
		button.SetTimer(num, num2);
		button.Set(action.Value.SkillId, state2);
	}

	public float? SetTameAction(DamageableEntity target)
	{
		BattleActionButton button = _tameActinoSlot.Button;
		if (PlayerBehavior.LocalPlayer.Level <= Yaml.Util.Singleton<Constants>.Instance.NewbieLevel)
		{
			button.Set(null, BattleActionButton.ButtonState.Empty);
			return null;
		}
		float? result = null;
		bool flag = false;
		foreach (ItemData playerItem in GameSystem<InventorySystem>.Instance().PlayerItemList)
		{
			if (playerItem.IsCapturable())
			{
				flag = true;
				break;
			}
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double num = GameSystem<CombatSystem>.Instance().UsingAction.TamingBeginAt;
		double num2 = num + (double)Yaml.Util.Singleton<Constants>.Instance.Taming.TamingCooltime;
		button.SetTimer(num, num2);
		button.SetIcon((!flag) ? "act_capture_x" : "act_capture_1");
		bool flag2 = false;
		BattleActionButton.ButtonState buttonState;
		if (num <= predictedServerTime && predictedServerTime < num2)
		{
			buttonState = BattleActionButton.ButtonState.Cooldown;
			result = Times.UnixTimeToUnityTime(num2);
		}
		else if (target == null || !target.IsAlive)
		{
			buttonState = BattleActionButton.ButtonState.Locked;
		}
		else if (TameableHelper.Instance().IsTameableType(target.GetEntityTypeId()))
		{
			AnimalBehavior component = target.GameObject.GetComponent<AnimalBehavior>();
			bool flag3 = false;
			if (component != null && component.Role == "warp_guard")
			{
				flag3 = true;
			}
			if (!flag3 && flag)
			{
				Gauge life = target.GetLife();
				float num3 = life.Ratio();
				float tamableHpRate = Yaml.Util.Singleton<Constants>.Instance.Taming.TamableHpRate;
				flag2 = num3 < tamableHpRate;
				buttonState = ((!flag2) ? BattleActionButton.ButtonState.Locked : BattleActionButton.ButtonState.Emphasis);
				double num4 = life.When(tamableHpRate, Gauge.CurrentTime);
				if (num4 > 0.0)
				{
					double num5 = num4 - Gauge.CurrentTime;
					result = Time.time + (float)num5;
				}
			}
			else
			{
				buttonState = BattleActionButton.ButtonState.Locked;
			}
		}
		else
		{
			buttonState = BattleActionButton.ButtonState.Sealed;
		}
		BattleActionButton.ButtonState state = button.State;
		button.Set(null, buttonState);
		if (flag2 && state != buttonState)
		{
			SoundManager.PlayEvent("ui_capture_button_click");
		}
		return result;
	}

	public BattleActionButton GetActionButton(int index)
	{
		if (_playerActionSlots == null || index < 0 || index >= _playerActionSlots.Length)
		{
			return null;
		}
		return _playerActionSlots[index].Button;
	}

	public BattleActionButton GetActionButton(string id)
	{
		SlotArgument[] playerActionSlots = _playerActionSlots;
		foreach (SlotArgument slotArgument in playerActionSlots)
		{
			if (slotArgument.Button.Id == id)
			{
				return slotArgument.Button;
			}
		}
		return null;
	}
}
