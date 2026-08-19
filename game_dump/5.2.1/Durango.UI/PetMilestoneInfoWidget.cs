using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.UI.Popup;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PetMilestoneInfoWidget : UIWidget, IUIInitializable
{
	[SerializeField]
	private UIWidget _milestoneWidget;

	[SerializeField]
	private PetMilestoneNodeWidget _focusedNode;

	[SerializeField]
	private PetMilestoneNodeWidget _normalBase;

	[SerializeField]
	private PetMilestoneNodeWidget _learnActiveFocusedNode;

	[SerializeField]
	private PetMilestoneNodeWidget _learnActiveNormalNode;

	[SerializeField]
	private UISprite _lineBase;

	[SerializeField]
	private ListObjectPool _activeSkills;

	[SerializeField]
	private GameObject _helpButton;

	[SerializeField]
	private GameObject _reRollButton;

	[SerializeField]
	private UIWidget _tooltip;

	[SerializeField]
	private RectLayoutComponent _tooltipRectLayout;

	private ListObjectPool<PetMilestoneNodeWidget> _normalPool;

	private ListObjectPool<UISprite> _linePool;

	private UIWidget _viwerWidget;

	private readonly List<UIWidget> _nodes = new List<UIWidget>();

	private Messages.Pet _pet;

	private MilestoneInfo? _focusedMilestone;

	public event Action<Messages.Pet, int> MilestonePicked;

	public event Action<Messages.Pet> ActiveSkillPicked;

	public event Action<Messages.Pet> MilestoneHelpClicked;

	void IUIInitializable.Init()
	{
		_normalPool = new ListObjectPool<PetMilestoneNodeWidget>();
		_linePool = new ListObjectPool<UISprite>();
		_normalPool.BaseObject = _normalBase;
		_normalPool.UseBase = true;
		_linePool.BaseObject = _lineBase;
		_linePool.UseBase = true;
		_viwerWidget = _focusedNode.transform.parent.GetComponent<UIWidget>();
		_activeSkills.Init(delegate(GameObject obj)
		{
			UIEventListener uIEventListener5 = UIEventListener.Get(obj);
			uIEventListener5.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener5.onClick, new UIEventListener.VoidDelegate(OnClickActiveSkill));
		});
		UIEventListener uIEventListener = UIEventListener.Get(_focusedNode.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickGetMilestone));
		UIEventListener uIEventListener2 = UIEventListener.Get(_learnActiveFocusedNode.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickLearnActiveAction));
		UIEventListener uIEventListener3 = UIEventListener.Get(_helpButton.gameObject);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (this.MilestoneHelpClicked != null)
			{
				this.MilestoneHelpClicked(_pet);
			}
		});
		UIEventListener uIEventListener4 = UIEventListener.Get(_reRollButton.gameObject);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, new UIEventListener.VoidDelegate(OnClickLearnActiveAction));
	}

	public void Set(Messages.Pet pet)
	{
		_pet = pet;
		if (_pet.Statistics.MilestonesInformation == null)
		{
			_milestoneWidget.gameObject.SetActive(value: false);
			_activeSkills.Clear();
			return;
		}
		MilestoneInfo[] milestonesInformation = _pet.Statistics.MilestonesInformation;
		if (KUtility.GetSize(_pet.Statistics.AvailableActiveSkill) > 0)
		{
			_milestoneWidget.gameObject.SetActive(value: false);
			_activeSkills.BeginLoad();
			for (int i = 0; i < _pet.Statistics.AvailableActiveSkill.Length; i++)
			{
				Messages.PetActiveSkill petActiveSkill = _pet.Statistics.AvailableActiveSkill[i];
				Yaml.PetActiveSkill petActiveSkill2 = PetActiveSkills.Get(petActiveSkill.SkillId, petActiveSkill.Rank);
				if (petActiveSkill2 != null)
				{
					GameObject next = _activeSkills.GetNext();
					UISprite component = next.transform.Find("Icon").GetComponent<UISprite>();
					UILabel component2 = next.transform.Find("Text").GetComponent<UILabel>();
					component.spriteName = petActiveSkill2.Icon;
					component2.text = petActiveSkill2.Name;
				}
			}
			_reRollButton.gameObject.SetActive(value: true);
			_activeSkills.EndLoad();
			UIUtility.WidgetsReposition(_activeSkills, Vector3.right, Vector3.zero, 5f, 0.5f);
			return;
		}
		_milestoneWidget.gameObject.SetActive(value: true);
		_activeSkills.Clear();
		_reRollButton.gameObject.SetActive(value: false);
		BeginLoad();
		_focusedMilestone = null;
		bool flag = PetUtil.PetReadyToDrawActiveSkill(pet);
		int currentPetMilestoneIndex = PetUtil.GetCurrentPetMilestoneIndex(pet);
		for (int j = 0; j < KUtility.GetSize(milestonesInformation); j++)
		{
			MilestoneInfo milestoneInfo = milestonesInformation[j];
			if (j < currentPetMilestoneIndex || (currentPetMilestoneIndex == milestonesInformation.Length - 1 && flag))
			{
				PetMilestoneNodeWidget next2 = _normalPool.GetNext();
				next2.Set(milestoneInfo);
				_nodes.Add(next2);
			}
			else if (j == currentPetMilestoneIndex)
			{
				_focusedMilestone = milestoneInfo;
				PetStatistics statistics = _pet.Statistics;
				int num = ((j <= 0) ? 1 : milestonesInformation[j - 1].Level);
				int num2 = 0;
				int num3 = 0;
				PetExpTable petExpTable = SingletonDict<int, PetExpTable>.Get(pet.EntityType);
				if (petExpTable != null)
				{
					for (int k = num; k < milestoneInfo.Level; k++)
					{
						int requiredExp = petExpTable.GetRequiredExp(k);
						num2 += requiredExp;
						if (k < statistics.Level)
						{
							num3 += requiredExp;
						}
						else if (k == statistics.Level)
						{
							num3 += statistics.Exp;
						}
					}
				}
				_focusedNode.SetProgress(milestoneInfo, num3, num2, _pet.Statistics.Level);
				_nodes.Add(_focusedNode);
				_tooltip.gameObject.SetActive(milestoneInfo.Level < _pet.Statistics.Level);
				_tooltipRectLayout.UpdateLayout();
			}
			else
			{
				PetMilestoneNodeWidget next3 = _normalPool.GetNext();
				next3.Set(milestoneInfo);
				_nodes.Add(next3);
			}
		}
		_nodes.Add((!flag) ? _learnActiveNormalNode : _learnActiveFocusedNode);
		EndLoad();
	}

	private void BeginLoad()
	{
		_nodes.Clear();
		_focusedNode.gameObject.SetActive(value: false);
		_learnActiveFocusedNode.gameObject.SetActive(value: false);
		_learnActiveNormalNode.gameObject.SetActive(value: false);
		_normalPool.BeginLoad();
	}

	private void EndLoad()
	{
		_normalPool.EndLoad();
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		if (_nodes.Count == 0)
		{
			return;
		}
		for (int i = 0; i < _nodes.Count; i++)
		{
			_nodes[i].gameObject.SetActive(value: true);
		}
		if (_nodes.Count > 1)
		{
			float num = 0f;
			for (int j = 0; j < _nodes.Count; j++)
			{
				num += (float)_nodes[j].width;
			}
			float num2 = ((float)_viwerWidget.width - num) / (float)(_nodes.Count - 1);
			Vector3[] array = _viwerWidget.localCorners;
			Vector3 vector = Vector3.Lerp(array[0], array[1], 0.5f);
			_linePool.BeginLoad();
			for (int k = 0; k < _nodes.Count; k++)
			{
				if (k > 0)
				{
					if (num2 > 10f)
					{
						UISprite next = _linePool.GetNext();
						next.width = (int)(num2 - 10f);
						next.SetPosition(vector + Vector3.right * 10f * 0.5f, 0f, 0.5f);
					}
					vector.x += num2;
				}
				_nodes[k].SetPosition(vector, 0f, 0.5f);
				vector.x += _nodes[k].width;
			}
			_linePool.EndLoad();
		}
		else
		{
			_linePool.Clear();
			_nodes[0].SetPosition(_viwerWidget.localCenter, 0.5f, 0.5f);
		}
	}

	private void OnClickGetMilestone(GameObject obj)
	{
		MilestoneInfo? focusedMilestone = _focusedMilestone;
		if (focusedMilestone.HasValue && _pet.Statistics.Level >= _focusedMilestone.Value.Level && this.MilestonePicked != null)
		{
			this.MilestonePicked(_pet, _focusedMilestone.Value.MilestoneTableId);
		}
	}

	private void OnClickLearnActiveAction(GameObject obj)
	{
		if (this.ActiveSkillPicked != null)
		{
			this.ActiveSkillPicked(_pet);
		}
	}

	private void OnClickActiveSkill(GameObject obj)
	{
		int num = _activeSkills.IndexOf(obj);
		if (num == -1)
		{
			return;
		}
		int num2 = 0;
		for (int i = 0; i < _pet.Statistics.AvailableActiveSkill.Length; i++)
		{
			Messages.PetActiveSkill petActiveSkill = _pet.Statistics.AvailableActiveSkill[i];
			Yaml.PetActiveSkill petActiveSkill2 = PetActiveSkills.Get(petActiveSkill.SkillId, petActiveSkill.Rank);
			if (petActiveSkill2 != null)
			{
				if (num == num2)
				{
					WidgetTooltipControl widgetTooltipControl = UIManager.Popup.FindTooltip<WidgetTooltipControl>();
					widgetTooltipControl.AutoPosition = false;
					widgetTooltipControl.Set($"<em>{petActiveSkill2.Name}</em>", petActiveSkill2.Description, 400);
					widgetTooltipControl.Show();
					widgetTooltipControl.SetPosition(obj.GetComponent<UIWidget>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), Vector3.up * 20f);
					break;
				}
				num2++;
			}
		}
	}
}
