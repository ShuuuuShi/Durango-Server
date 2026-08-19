using System;
using System.Collections.Generic;
using Shared.Battle;
using UnityEngine;

public class ActionButtonContainer : MonoBehaviour
{
	[SerializeField]
	private ActionButton _actionButtonBase;

	[SerializeField]
	private Vector3[] _actionSlotPositionsBattle;

	private readonly List<ActionButton> _actionButtons = new List<ActionButton>();

	private double _autoActionReservedTime = -1.0;

	public event Action<string> ActionClicked;

	private void Awake()
	{
		((Component)_actionButtonBase).gameObject.SetActive(false);
		ReserveActionButtons();
	}

	private void ReserveActionButtons()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		int num = _actionSlotPositionsBattle.Length;
		for (int i = 0; i < num; i++)
		{
			GameObject val = ((Component)this).gameObject.AddChild(((Component)_actionButtonBase).gameObject);
			((Object)val).name = $"ActiveAction_{i}";
			val.transform.localPosition = _actionSlotPositionsBattle[i];
			ActionButton component = val.GetComponent<ActionButton>();
			_actionButtons.Add(component);
			component.Hide();
		}
	}

	public void InitIconActionButtons(IList<string> ids, IList<ActionGroup> actionGroups, IList<string> icons)
	{
		InitActionButtons(ids, actionGroups, null, icons);
	}

	private int GetInitialPositionIndex(ActionGroup actionGroup)
	{
		return actionGroup switch
		{
			ActionGroup.ClientSided => 0, 
			ActionGroup.Normal => 1, 
			ActionGroup.Counter => -1, 
			ActionGroup.Guard => 0, 
			ActionGroup.ActiveAction => 2, 
			ActionGroup.Tackle => 5, 
			ActionGroup.Additional => 6, 
			_ => -1, 
		};
	}

	public void InitActionButtons(IList<string> ids, IList<ActionGroup> actionGroups, IList<string> texts, IList<string> icons)
	{
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] actionSlotPositionsBattle = _actionSlotPositionsBattle;
		if (ids == null || actionSlotPositionsBattle == null)
		{
			return;
		}
		int size = KUtility.GetSize(texts);
		int size2 = KUtility.GetSize(icons);
		if (size == 0 && size2 == 0)
		{
			return;
		}
		int num = Mathf.Min(new int[3]
		{
			ids.Count,
			Mathf.Max(size, size2),
			actionSlotPositionsBattle.Length
		});
		int num2 = 0;
		int[] array = new int[7];
		for (int i = 0; i < 7; i++)
		{
			array[i] = -1;
		}
		Vector3 val = default(Vector3);
		for (int j = 0; j < num; j++)
		{
			if (ids[j] != null)
			{
				ActionButton actionButton = _actionButtons[num2];
				bool flag = actionButton.Id != ids[j];
				actionButton.Id = ids[j];
				actionButton.ActionGroup = actionGroups?[j] ?? ActionGroup.ClientSided;
				array[(int)actionButton.ActionGroup]++;
				if (j < size && texts[j] != null)
				{
					actionButton.Text = texts[j];
				}
				if (j < size2 && icons[j] != null)
				{
					actionButton.IconName = icons[j];
				}
				else
				{
					actionButton.Text = string.Empty;
				}
				if (actionButton.IsAutoAction())
				{
					actionButton.IconName = GameSystem<EquipSystem>.Instance().Weapon.Icon;
					Vector3 localScale = ((Component)actionButton.IconSprite).transform.localScale;
					localScale.x = -1f;
					((Component)actionButton.IconSprite).transform.localScale = localScale;
					actionButton.Text = string.Empty;
				}
				actionButton.Listener.onClick = OnClickActionButton;
				int num3 = GetInitialPositionIndex(actionButton.ActionGroup) + array[(int)actionButton.ActionGroup];
				Vector3 localPosition = actionSlotPositionsBattle[num3];
				((Vector3)(ref val))._002Ector(localPosition.z, localPosition.z, localPosition.z);
				localPosition.z = 0f;
				((Component)actionButton).transform.localPosition = localPosition;
				if (flag)
				{
					float delay = (float)num2 * 0.1f + 0.2f;
					actionButton.AppearEffectScaleTweener.tweenFactor = 0f;
					actionButton.AppearEffectScaleTweener.delay = delay;
					actionButton.AppearEffectScaleTweener.to = val;
					actionButton.AppearEffectScaleTweener.PlayForward();
					actionButton.ReservedScaleTweener.SetInitScale(val);
					actionButton.WidgetAlphaTweener.tweenFactor = 0f;
					actionButton.WidgetAlphaTweener.delay = delay;
					actionButton.WidgetAlphaTweener.PlayForward();
					actionButton.Widget.alpha = 0f;
				}
				else if (((Behaviour)actionButton.WidgetAlphaTweener).enabled)
				{
					actionButton.WidgetAlphaTweener.to = actionButton.Alpha;
				}
				else
				{
					actionButton.Widget.alpha = actionButton.Alpha;
				}
				num2++;
			}
		}
		for (int k = num2; k < _actionButtons.Count; k++)
		{
			ActionButton actionButton2 = _actionButtons[k];
			actionButton2.Hide();
		}
	}

	public ActionButton FindActionButton(string id)
	{
		int i = 0;
		for (int count = _actionButtons.Count; i < count; i++)
		{
			if (_actionButtons[i].Id == id)
			{
				return _actionButtons[i];
			}
		}
		return null;
	}

	public void SetActionButtonState(string id, ActionState state)
	{
		ActionButton actionButton = FindActionButton(id);
		if ((Object)(object)actionButton != (Object)null)
		{
			actionButton.CurState = state;
		}
	}

	public void SetActionButtonDeactiveTime(string id, double since, double until)
	{
		ActionButton actionButton = FindActionButton(id);
		if ((Object)(object)actionButton != (Object)null)
		{
			actionButton.SetDeactiveTime(since, until);
		}
	}

	public void HideAllActionButtons()
	{
		int i = 0;
		for (int count = _actionButtons.Count; i < count; i++)
		{
			_actionButtons[i].PlayOrStopReservedAnim(reserved: false);
			_actionButtons[i].Hide();
		}
	}

	private void OnClickActionButton(GameObject go)
	{
		ActionButton component = go.GetComponent<ActionButton>();
		if ((Object)(object)component != (Object)null && ((Component)component).gameObject.activeSelf && component.IsClickable && this.ActionClicked != null)
		{
			this.ActionClicked(component.Id);
		}
	}

	public void ReserveAction(string actionKey)
	{
		_autoActionReservedTime = -1.0;
		int i = 0;
		for (int count = _actionButtons.Count; i < count; i++)
		{
			_actionButtons[i].PlayOrStopReservedAnim(_actionButtons[i].Id == actionKey);
		}
	}

	public ActionButton FindAutoActionButton()
	{
		int i = 0;
		for (int count = _actionButtons.Count; i < count; i++)
		{
			if (_actionButtons[i].IsAutoAction())
			{
				return _actionButtons[i];
			}
		}
		return null;
	}

	public void ReserveAutoAction(double at)
	{
		_autoActionReservedTime = at;
	}

	private void Update()
	{
		ProcessAutoActionReserving();
	}

	private void ProcessAutoActionReserving()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (_autoActionReservedTime > 0.0 && predictedServerTime >= _autoActionReservedTime)
		{
			ActionButton actionButton = FindAutoActionButton();
			if (Object.op_Implicit((Object)(object)actionButton))
			{
				ReserveAction(actionButton.Id);
			}
			_autoActionReservedTime = -1.0;
		}
	}
}
