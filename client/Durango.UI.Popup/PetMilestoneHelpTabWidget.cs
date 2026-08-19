using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class PetMilestoneHelpTabWidget : MonoBehaviour, IUIInitializable
{
	public Action TipClicked;

	public Action ActiveSkillClicked;

	public Action<MilestoneInfo> MilestoneClicked;

	[SerializeField]
	private SelectableWidget _tipButton;

	[SerializeField]
	private SelectableWidget _milestoneButtonBase;

	[SerializeField]
	private SelectableWidget _activeSkillButton;

	[SerializeField]
	private TweenerPlayer _selectedEffect;

	[SerializeField]
	private UISprite _lineBase;

	private ListObjectPool<UISprite> _linePool;

	private ListObjectPool<SelectableWidget> _milestonButtons;

	private readonly List<UIWidget> _layoutWidgets = new List<UIWidget>();

	private Pet _pet;

	void IUIInitializable.Init()
	{
		SetButtonText(_tipButton, T._("TIP"));
		_milestonButtons = new ListObjectPool<SelectableWidget>();
		_linePool = new ListObjectPool<UISprite>();
		_milestonButtons.BaseObject = _milestoneButtonBase;
		_milestonButtons.UseBase = true;
		_linePool.BaseObject = _lineBase;
		_linePool.UseBase = true;
		SelectableWidget tipButton = _tipButton;
		tipButton.Clicked = (Action)Delegate.Combine(tipButton.Clicked, (Action)delegate
		{
			if (TipClicked != null)
			{
				TipClicked();
			}
		});
		SelectableWidget activeSkillButton = _activeSkillButton;
		activeSkillButton.Clicked = (Action)Delegate.Combine(activeSkillButton.Clicked, (Action)delegate
		{
			if (ActiveSkillClicked != null)
			{
				ActiveSkillClicked();
			}
		});
		_milestonButtons.Init(delegate(SelectableWidget obj)
		{
			obj.Clicked = (Action)Delegate.Combine(obj.Clicked, new Action(OnClickMilestoneButton));
		});
	}

	private void OnClickMilestoneButton()
	{
		int num = _milestonButtons.IndexOf(Selectable.Current as SelectableWidget);
		if (num != -1)
		{
			MilestoneInfo[] milestonesInformation = _pet.Statistics.MilestonesInformation;
			if (milestonesInformation != null && milestonesInformation.TryGet(num, out var element) && MilestoneClicked != null)
			{
				MilestoneClicked(element);
			}
		}
	}

	public void Set(Pet pet)
	{
		_pet = pet;
		_layoutWidgets.Clear();
		_layoutWidgets.Add(_tipButton.Widget);
		_milestonButtons.BeginLoad();
		MilestoneInfo[] milestonesInformation = pet.Statistics.MilestonesInformation;
		if (milestonesInformation != null)
		{
			MilestoneInfo[] array = milestonesInformation;
			for (int i = 0; i < array.Length; i++)
			{
				MilestoneInfo milestoneInfo = array[i];
				SelectableWidget next = _milestonButtons.GetNext();
				SetButtonText(next, LocalizeUtil.FormatLevel(milestoneInfo.Level));
				_layoutWidgets.Add(next.Widget);
			}
		}
		_milestonButtons.EndLoad();
		_layoutWidgets.Add(_activeSkillButton.Widget);
		UpdateLayout();
	}

	public void SelectTip()
	{
		_tipButton.Selected = true;
		_activeSkillButton.Selected = false;
		foreach (SelectableWidget milestonButton in _milestonButtons)
		{
			milestonButton.Selected = false;
		}
		_selectedEffect.transform.localPosition = _tipButton.transform.localPosition;
		_selectedEffect.Play();
	}

	public void SelectActiveSkill()
	{
		_tipButton.Selected = false;
		_activeSkillButton.Selected = true;
		foreach (SelectableWidget milestonButton in _milestonButtons)
		{
			milestonButton.Selected = false;
		}
		_selectedEffect.transform.localPosition = _activeSkillButton.transform.localPosition;
		_selectedEffect.Play();
	}

	public void SelectMilestone(int index)
	{
		_tipButton.Selected = false;
		_activeSkillButton.Selected = false;
		for (int i = 0; i < _milestonButtons.Count; i++)
		{
			_milestonButtons[i].Selected = i == index;
		}
		_selectedEffect.transform.localPosition = _milestonButtons[index].transform.localPosition;
		_selectedEffect.Play();
	}

	private void SetButtonText(SelectableWidget button, string text)
	{
		UILabel component = button.transform.Find("Label").GetComponent<UILabel>();
		component.text = text;
	}

	private void UpdateLayout()
	{
		if (_layoutWidgets.Count == 0)
		{
			_linePool.Clear();
			return;
		}
		for (int i = 0; i < _layoutWidgets.Count; i++)
		{
			_layoutWidgets[i].gameObject.SetActive(value: true);
		}
		UIWidget component = GetComponent<UIWidget>();
		if (_layoutWidgets.Count > 1)
		{
			float num = 0f;
			for (int j = 0; j < _layoutWidgets.Count; j++)
			{
				num += (float)_layoutWidgets[j].width;
			}
			float num2 = ((float)component.width - 60f - num) / (float)(_layoutWidgets.Count - 1);
			Vector3[] localCorners = component.localCorners;
			Vector3 vector = Vector3.Lerp(localCorners[0], localCorners[1], 0.5f);
			vector.x += 30f;
			_linePool.BeginLoad();
			for (int k = 0; k < _layoutWidgets.Count; k++)
			{
				if (k > 0)
				{
					if (num2 > -45f)
					{
						UISprite next = _linePool.GetNext();
						next.width = (int)(num2 - -45f);
						next.SetPosition(vector + Vector3.right * -45f * 0.5f, 0f, 0.5f);
					}
					vector.x += num2;
				}
				_layoutWidgets[k].SetPosition(vector, 0f, 0.5f);
				vector.x += _layoutWidgets[k].width;
			}
			_linePool.EndLoad();
		}
		else
		{
			_linePool.Clear();
			_layoutWidgets[0].SetPosition(component.localCenter, 0.5f, 0.5f);
		}
	}
}
