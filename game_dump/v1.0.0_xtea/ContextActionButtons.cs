using System;
using System.Collections.Generic;
using InteractionData;
using UnityEngine;

public class ContextActionButtons : MonoBehaviour
{
	private struct CooltimeStruct
	{
		public Interaction Key;

		public double DeactiveAt;

		public double ReactiveAt;
	}

	[SerializeField]
	private ContextActionButton _actionButtonBase;

	[SerializeField]
	private Vector3[] _actionSlotPositions;

	private ListObjectPool<ContextActionButton> _actionButtons;

	private List<CooltimeStruct> _cooltimeList = new List<CooltimeStruct>();

	private bool _isInit;

	public event Action<Interaction> ActionClicked;

	private void Init()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_actionButtons = new ListObjectPool<ContextActionButton>();
			_actionButtons.BaseObject = _actionButtonBase;
			_actionButtons.Init(delegate(ContextActionButton button)
			{
				UIEventListener uIEventListener = UIEventListener.Get(((Component)button).gameObject);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickActionButton));
			});
			int num = _actionSlotPositions.Length;
			_actionButtons.Set(num);
			for (int i = 0; i < num; i++)
			{
				ContextActionButton contextActionButton = _actionButtons[i];
				((Component)contextActionButton).transform.localPosition = _actionSlotPositions[i];
				contextActionButton.Hide();
			}
		}
	}

	private void Start()
	{
		Init();
	}

	public void SetActions(IList<Interaction> ids)
	{
		Init();
		Vector3[] actionSlotPositions = _actionSlotPositions;
		if (ids == null || actionSlotPositions == null)
		{
			return;
		}
		int num = Mathf.Min(ids.Count, actionSlotPositions.Length);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			ContextActionButton contextActionButton = _actionButtons[num2];
			contextActionButton.Show(ids[i]);
			if (TryGetActionCooltime(ids[i], out var cooltime))
			{
				contextActionButton.SetCooltime(cooltime.DeactiveAt, cooltime.ReactiveAt);
			}
			else
			{
				contextActionButton.SetCooltime(0.0, 0.0);
			}
			num2++;
		}
		for (int j = num2; j < _actionButtons.Count; j++)
		{
			ContextActionButton contextActionButton2 = _actionButtons[j];
			contextActionButton2.Hide();
		}
	}

	public void Hide()
	{
		int i = 0;
		for (int count = _actionButtons.Count; i < count; i++)
		{
			_actionButtons[i].Hide();
		}
	}

	private void OnClickActionButton(GameObject obj)
	{
		ContextActionButton component = obj.GetComponent<ContextActionButton>();
		if (((Object)(object)component != (Object)null) & (this.ActionClicked != null))
		{
			this.ActionClicked(component.Action);
		}
	}

	private ContextActionButton GetActionButton(Interaction key)
	{
		for (int i = 0; i < _actionButtons.Count; i++)
		{
			if (_actionButtons[i].Action == key)
			{
				return _actionButtons[i];
			}
		}
		return null;
	}

	private bool TryGetActionCooltime(Interaction key, out CooltimeStruct cooltime)
	{
		for (int i = 0; i < _cooltimeList.Count; i++)
		{
			if (_cooltimeList[i].Key == key)
			{
				cooltime = _cooltimeList[i];
				return true;
			}
		}
		cooltime = default(CooltimeStruct);
		return false;
	}

	public void SetActionCooltime(Interaction key, double deactiveAt, double reactiveAt)
	{
		_cooltimeList.Add(new CooltimeStruct
		{
			Key = key,
			DeactiveAt = deactiveAt,
			ReactiveAt = reactiveAt
		});
		ContextActionButton actionButton = GetActionButton(key);
		if ((Object)(object)actionButton == (Object)null)
		{
			return;
		}
		actionButton.SetCooltime(deactiveAt, reactiveAt);
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		for (int num = _cooltimeList.Count - 1; num >= 0; num--)
		{
			if (_cooltimeList[num].ReactiveAt < predictedServerTime)
			{
				_cooltimeList.RemoveAt(num);
			}
		}
	}
}
