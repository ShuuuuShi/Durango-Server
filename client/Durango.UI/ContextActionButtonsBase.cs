using System;
using System.Collections.Generic;
using Durango.Network;
using InteractionData;
using UnityEngine;

namespace Durango.UI;

public class ContextActionButtonsBase : MonoBehaviour
{
	private struct CooltimeStruct
	{
		public Interaction Key;

		public string Argument;

		public double Since;

		public double Until;
	}

	[SerializeField]
	protected Vector3[] _actionSlotPositions;

	protected ListObjectPool<ContextActionButtonBase> _actionButtons;

	[SerializeField]
	private ContextActionButtonBase _actionButtonBase;

	private readonly List<CooltimeStruct> _cooltimes = new List<CooltimeStruct>();

	private bool _isInit;

	public event Action<InteractionMenuData> MenuClicked;

	public event Action<ContextActionButtonBase, bool> MenuPressed;

	public event Action<ContextActionButtonBase, bool> MenuHovered;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_actionButtons = new ListObjectPool<ContextActionButtonBase>();
			_actionButtons.BaseObject = _actionButtonBase;
			_actionButtons.Init(delegate(ContextActionButtonBase button)
			{
				button.Clicked += OnClickActionButton;
				button.Pressed += OnPressedActionButton;
				button.Hovered += OnHoveredActionButton;
			});
			int num = _actionSlotPositions.Length;
			_actionButtons.Set(num);
			for (int i = 0; i < num; i++)
			{
				ContextActionButtonBase contextActionButtonBase = _actionButtons[i];
				contextActionButtonBase.transform.localPosition = _actionSlotPositions[i];
				contextActionButtonBase.Hide();
			}
		}
	}

	protected virtual void Start()
	{
		Init();
	}

	private void Update()
	{
		for (int i = 0; i < _actionButtons.Count; i++)
		{
			_actionButtons[i].UpdateRoutine();
		}
	}

	public virtual void SetActions(List<InteractionMenuData> menus)
	{
		Init();
		Vector3[] actionSlotPositions = _actionSlotPositions;
		if (menus == null || actionSlotPositions == null)
		{
			return;
		}
		int num = Mathf.Min(menus.Count, actionSlotPositions.Length);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			ContextActionButtonBase contextActionButtonBase = _actionButtons[num2];
			contextActionButtonBase.Show(menus[i]);
			int num3 = ActionCooltimeIndexOf(menus[i].Action, menus[i].Id);
			if (num3 == -1)
			{
				contextActionButtonBase.SetCooltime(0.0, 0.0);
			}
			else
			{
				CooltimeStruct cooltimeStruct = _cooltimes[num3];
				contextActionButtonBase.SetCooltime(cooltimeStruct.Since, cooltimeStruct.Until);
			}
			num2++;
		}
		for (int j = num2; j < _actionButtons.Count; j++)
		{
			ContextActionButtonBase contextActionButtonBase2 = _actionButtons[j];
			contextActionButtonBase2.Hide();
		}
	}

	protected void OnClickActionButton(ContextActionButtonBase btn)
	{
		if ((btn != null) & (this.MenuClicked != null))
		{
			this.MenuClicked(btn.Menu);
		}
	}

	protected void OnPressedActionButton(ContextActionButtonBase btn, bool pressed)
	{
		if (btn != null && this.MenuPressed != null)
		{
			this.MenuPressed(btn, pressed);
		}
	}

	protected void OnHoveredActionButton(ContextActionButtonBase btn, bool hovered)
	{
		if ((btn != null) & (this.MenuHovered != null))
		{
			this.MenuHovered(btn, hovered);
		}
	}

	public ContextActionButtonBase GetActionButton(Interaction key)
	{
		int actionButtonIndex = GetActionButtonIndex(key);
		return (actionButtonIndex == -1) ? null : _actionButtons[actionButtonIndex];
	}

	public ContextActionButtonBase GetActionButton(Interaction key, string argument)
	{
		int actionButtonIndex = GetActionButtonIndex(key, argument);
		return (actionButtonIndex == -1) ? null : _actionButtons[actionButtonIndex];
	}

	public int GetActionButtonIndex(Interaction key)
	{
		for (int i = 0; i < _actionButtons.Count; i++)
		{
			if (_actionButtons[i].Menu.Action == key)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetActionButtonIndex(Interaction key, string argument)
	{
		for (int i = 0; i < _actionButtons.Count; i++)
		{
			if (_actionButtons[i].Menu.IsEqualKey(key, argument))
			{
				return i;
			}
		}
		return -1;
	}

	private int ActionCooltimeIndexOf(Interaction key, string argument)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		for (int num = _cooltimes.Count - 1; num >= 0; num--)
		{
			if (_cooltimes[num].Until < predictedServerTime)
			{
				_cooltimes.RemoveAt(num);
			}
		}
		int num2 = -1;
		for (int num3 = _cooltimes.Count - 1; num3 >= 0; num3--)
		{
			CooltimeStruct cooltimeStruct = _cooltimes[num3];
			if (num2 == -1 && cooltimeStruct.Key == key && cooltimeStruct.Argument == argument)
			{
				num2 = num3;
			}
		}
		return num2;
	}

	public void SetActionCooltime(Interaction key, string argument, double since, double until)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		int num = ActionCooltimeIndexOf(key, argument);
		if (predictedServerTime < until)
		{
			CooltimeStruct cooltimeStruct = default(CooltimeStruct);
			cooltimeStruct.Key = key;
			cooltimeStruct.Argument = argument;
			cooltimeStruct.Since = since;
			cooltimeStruct.Until = until;
			CooltimeStruct cooltimeStruct2 = cooltimeStruct;
			if (num == -1)
			{
				_cooltimes.Add(cooltimeStruct2);
			}
			else
			{
				_cooltimes[num] = cooltimeStruct2;
			}
		}
		else if (num != -1)
		{
			_cooltimes.RemoveAt(num);
		}
		ContextActionButtonBase actionButton = GetActionButton(key, argument);
		if (!(actionButton == null))
		{
			actionButton.SetCooltime(since, until);
		}
	}

	public void ClearActionCooltime(Interaction key, string argument)
	{
		SetActionCooltime(key, argument, 0.0, 0.0);
	}
}
