using System.Collections.Generic;
using Durango.Logic.Interactions;
using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class InteractionHelperList_PC : InteractionHelperList
{
	[SerializeField]
	private InteractionHelperLabelKey _deadKey;

	[SerializeField]
	[Tooltip("사망시 단축키 표기 위치")]
	private Vector2 _deadKeyPosition;

	[SerializeField]
	private float _recalcPeriod;

	private float _recalculateAt;

	public override void Init()
	{
		base.Init();
		_deadKey.SetShortcut(InputCommand.Collect, null);
		base.ShowStateChanged += delegate
		{
			ShowShortcutOnDeadBody(base.IsShow);
			if (base.IsShow)
			{
				return;
			}
			foreach (InteractionHelperLabel helper in Helpers)
			{
				EnableHotKey(helper, enable: false);
			}
		};
		GameSystem<InputSystem>.Instance().On(InputCommand.Collect, OnClickCollectKey);
	}

	protected override void RefreshHelpers()
	{
		base.RefreshHelpers();
		SetupClosestHelper();
	}

	private void ShowShortcutOnDeadBody(bool isShow)
	{
		if (PlayerBehavior.LocalPlayer.IsAlive || !isShow)
		{
			_deadKey.gameObject.SetActive(value: false);
			return;
		}
		Vector3 floatingUIPosition = PlayerBehavior.LocalPlayer.FloatingUIPosition;
		floatingUIPosition = MainCamera.WorldToNGUIPos(floatingUIPosition);
		floatingUIPosition.x += _deadKeyPosition.x;
		floatingUIPosition.y += _deadKeyPosition.y;
		_deadKey.transform.localPosition = floatingUIPosition;
		_deadKey.gameObject.SetActive(value: true);
	}

	private void SetupClosestHelper()
	{
		_recalculateAt = Time.time + _recalcPeriod;
		if (Helpers.Count == 0)
		{
			return;
		}
		Vector3 vector = MainCamera.WorldToNGUIPos(PlayerBehavior.LocalPlayer.CurrentPosition);
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < Helpers.Count; i++)
		{
			if (Helpers[i].IsShow)
			{
				Vector3 localPosition = Helpers[i].transform.localPosition;
				localPosition.z = 0f;
				float sqrMagnitude = (vector - localPosition).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					num = i;
				}
			}
		}
		if (num != -1 && GameSystem<InteractionSystem>.Instance().Target == null && PlayerBehavior.LocalPlayer.IsAlive)
		{
			EnableHotKey(Helpers[num], enable: true);
		}
		for (int j = 0; j < Helpers.Count; j++)
		{
			if (Helpers[j].IsShow && j != num)
			{
				EnableHotKey(Helpers[j], enable: false);
			}
		}
	}

	protected override void OnClickHelperLabel()
	{
		if (!PlayerBehavior.LocalPlayer.IsAlive)
		{
			return;
		}
		foreach (InteractionHelperLabel helper in Helpers)
		{
			InteractionHelperLabel_PC interactionHelperLabel_PC = helper as InteractionHelperLabel_PC;
			if (interactionHelperLabel_PC != null && interactionHelperLabel_PC.HotKeyPressed)
			{
				Singleton<PlayerController>.Instance().StopMove();
				GameSystem<InteractionSystem>.Instance().SetInteractionTarget(new InteractionObject(interactionHelperLabel_PC.Target));
				return;
			}
		}
		base.OnClickHelperLabel();
	}

	private static void EnableHotKey(InteractionHelperLabel helper, bool enable)
	{
		InteractionHelperLabel_PC interactionHelperLabel_PC = helper as InteractionHelperLabel_PC;
		if (interactionHelperLabel_PC != null)
		{
			interactionHelperLabel_PC.EnableHotKey(enable);
		}
	}

	private void OnClickCollectKey(InputCommandMessage message)
	{
		if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			return;
		}
		if (!PlayerBehavior.LocalPlayer.IsAlive)
		{
			if (GameSystem<InteractionSystem>.Instance().Target == null)
			{
				UIManager.FindScript<InteractionGroup>().ShowPlayerDeadInteractionMenu();
			}
			else
			{
				GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
			}
		}
		else
		{
			if (base.IsShow || GameSystem<InteractionSystem>.Instance().Target != null)
			{
				return;
			}
			List<GameObject> list = ObjectBuffer;
			if (RefreshAt < Time.time)
			{
				RefreshAt = Time.time + RefreshPeriod;
				list = UpdateObjectBuffer();
			}
			if (list.Count == 0)
			{
				return;
			}
			Vector3 vector = MainCamera.WorldToNGUIPos(PlayerBehavior.LocalPlayer.CurrentPosition);
			TargetPosition targetPosition = new TargetPosition();
			int num = -1;
			float num2 = float.MaxValue;
			for (int i = 0; i < list.Count; i++)
			{
				targetPosition.Set(list[i]);
				if (!targetPosition.TryGet(out var pos))
				{
					return;
				}
				Vector3 vector2 = MainCamera.WorldToNGUIPos(pos);
				float sqrMagnitude = (vector - vector2).sqrMagnitude;
				if (sqrMagnitude < num2)
				{
					num2 = sqrMagnitude;
					num = i;
				}
			}
			if (num != -1)
			{
				Singleton<PlayerController>.Instance().StopMove();
				GameSystem<InteractionSystem>.Instance().SetInteractionTarget(new InteractionObject(list[num]));
			}
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (base.IsShow && _recalculateAt < Time.time)
		{
			SetupClosestHelper();
		}
	}
}
