using System;
using System.Collections.Generic;
using Durango.UI.Control;
using L10N;
using Messages;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI.Popup;

public class PetMilestoneHelpPage : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Selectable _graphTabButton;

	[SerializeField]
	private UIWidget _listViewContainer;

	[SerializeField]
	private KScrollView _listView;

	[SerializeField]
	private UIWidget _graphViewContainer;

	[SerializeField]
	private NestedPrefabLinker _graphLinker;

	[SerializeField]
	private PetMilestoneSelectedInfoWidget _focusedInfoWidget;

	[SerializeField]
	private UILabel _helpTextLabel;

	[SerializeField]
	private UILabel _ratioTextLabel;

	[SerializeField]
	private UIWidget _infoViewContainer;

	[SerializeField]
	private UILabel _infoLabel;

	private PetMilestoneRollWidget _rollWidget;

	private MilestoneCandidates _milestoneCandidates;

	private List<Pair<PetActiveSkill, float>> _activeSkillCandidates;

	public event Action GraphButtonClicked;

	void IUIInitializable.Init()
	{
		_rollWidget = _graphLinker.Object.GetComponent<PetMilestoneRollWidget>();
		_rollWidget.TagFocused += GraphTagFocused;
		_rollWidget.SkillFocused += GraphSkillFocused;
		_rollWidget.Unfocused += GraphUnfocused;
		Selectable graphTabButton = _graphTabButton;
		graphTabButton.Clicked = (Action)Delegate.Combine(graphTabButton.Clicked, (Action)delegate
		{
			if (this.GraphButtonClicked != null)
			{
				this.GraphButtonClicked();
			}
		});
	}

	public void ShowEmpty(bool isGraph)
	{
		_listViewContainer.gameObject.SetActive(value: false);
		_graphViewContainer.gameObject.SetActive(value: false);
		_infoViewContainer.gameObject.SetActive(value: false);
		_graphTabButton.Disabled = false;
		_graphTabButton.Selected = isGraph;
	}

	private void GraphTagFocused(string tagId)
	{
		_helpTextLabel.gameObject.SetActive(value: false);
		_focusedInfoWidget.Set(tagId);
		if (_milestoneCandidates.Result == null)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		Pair<string, float>[] result = _milestoneCandidates.Result;
		for (int i = 0; i < result.Length; i++)
		{
			Pair<string, float> pair = result[i];
			num2 += pair.Item2;
			if (pair.Item1 == tagId)
			{
				num = pair.Item2;
			}
		}
		if (num2 > 0f)
		{
			_ratioTextLabel.gameObject.SetActive(value: true);
			_ratioTextLabel.text = (num / num2).ToString("P1");
		}
		else
		{
			_ratioTextLabel.gameObject.SetActive(value: false);
		}
	}

	private void GraphSkillFocused(PetActiveSkill skill)
	{
		_helpTextLabel.gameObject.SetActive(value: false);
		_focusedInfoWidget.Set(skill);
		if (_activeSkillCandidates == null)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		foreach (Pair<PetActiveSkill, float> activeSkillCandidate in _activeSkillCandidates)
		{
			num2 += activeSkillCandidate.Item2;
			if (activeSkillCandidate.Item1.SkillId == skill.SkillId && activeSkillCandidate.Item1.Rank == skill.Rank)
			{
				num = activeSkillCandidate.Item2;
			}
		}
		if (num2 > 0f)
		{
			_ratioTextLabel.gameObject.SetActive(value: true);
			_ratioTextLabel.text = (num / num2).ToString("P1");
		}
		else
		{
			_ratioTextLabel.gameObject.SetActive(value: false);
		}
	}

	private void GraphUnfocused()
	{
		_helpTextLabel.gameObject.SetActive(value: true);
		_ratioTextLabel.gameObject.SetActive(value: false);
		_focusedInfoWidget.SetClear();
	}

	public void ShowTitle(string title)
	{
		_titleLabel.text = title;
	}

	public void ShowAcquiredMilestone(MilestoneInfo milestone, bool instant)
	{
		_graphViewContainer.gameObject.SetActive(value: false);
		_listViewContainer.gameObject.SetActive(value: false);
		_graphTabButton.Disabled = true;
		string arg = ((!string.IsNullOrEmpty(milestone.TagId)) ? $"<tag>{milestone.TagId}</tag>" : string.Format("<weak>{0}</weak>", T._("능력 없음")));
		_infoLabel.text = string.Format("{0}\n[size=16] [/size]\n{1}", T._("속성 발견을 완료했습니다."), arg);
		ShowPage(_infoViewContainer, instant);
	}

	public void ShowMilestoneCandidates(MilestoneInfo milestone, MilestoneCandidates candidates, bool isGraph, bool instant)
	{
		_infoViewContainer.gameObject.SetActive(value: false);
		_graphTabButton.Disabled = false;
		_milestoneCandidates = candidates;
		if (isGraph)
		{
			_helpTextLabel.text = string.Format("<em>{0}</em>\n[size=10] [/size]\n[size=22]{1}[/size]", LocalizeUtil.FormatLevel(milestone.Level), T._("속성 발견 확률"));
			_graphTabButton.Selected = true;
			ShowPage(_graphViewContainer, instant);
			_listViewContainer.gameObject.SetActive(value: false);
			_rollWidget.Show(candidates);
			GraphUnfocused();
			return;
		}
		_graphTabButton.Selected = false;
		ShowPage(_listViewContainer, instant);
		_graphViewContainer.gameObject.SetActive(value: false);
		_listView.Nodes.BeginLoad();
		if (candidates.Result != null)
		{
			int num = 0;
			Pair<string, float>[] result = candidates.Result;
			for (int i = 0; i < result.Length; i++)
			{
				Pair<string, float> pair = result[i];
				if (string.IsNullOrEmpty(pair.Item1))
				{
					continue;
				}
				PetMilestoneHelpItemWidget component = _listView.Nodes.GetNext().GetComponent<PetMilestoneHelpItemWidget>();
				float weight = 0f;
				int j = 0;
				for (int size = KUtility.GetSize(candidates.Original); j < size; j++)
				{
					if (candidates.Original[j].Item1 == pair.Item1)
					{
						weight = candidates.Original[j].Item2;
						break;
					}
				}
				num++;
				component.SetIndex(num);
				component.SetMiletone(pair.Item1, pair.Item2, weight);
			}
		}
		_listView.Nodes.EndLoad();
		_listView.ResetPosition();
	}

	public void ShowActiveSkillCandidates(List<Pair<PetActiveSkill, float>> activeSkillCandidates, bool isGraph, bool instant)
	{
		_infoViewContainer.gameObject.SetActive(value: false);
		_graphTabButton.Disabled = false;
		_activeSkillCandidates = activeSkillCandidates;
		if (isGraph)
		{
			_helpTextLabel.text = string.Format("<em>{0}</em>\n[size=6] [/size]\n[size=22]{1}[/size]", T._("특수 행동"), T._("발견 확률"));
			_graphTabButton.Selected = true;
			ShowPage(_graphViewContainer, instant);
			_listViewContainer.gameObject.SetActive(value: false);
			_rollWidget.Show(activeSkillCandidates);
			GraphUnfocused();
			return;
		}
		_graphTabButton.Selected = false;
		ShowPage(_listViewContainer, instant);
		_graphViewContainer.gameObject.SetActive(value: false);
		_listView.Nodes.BeginLoad();
		if (activeSkillCandidates != null)
		{
			int num = 0;
			foreach (Pair<PetActiveSkill, float> activeSkillCandidate in activeSkillCandidates)
			{
				PetMilestoneHelpItemWidget component = _listView.Nodes.GetNext().GetComponent<PetMilestoneHelpItemWidget>();
				num++;
				component.SetIndex(num);
				component.SetSkill(activeSkillCandidate.Item1);
			}
		}
		_listView.Nodes.EndLoad();
		_listView.ResetPosition();
	}

	private static void ShowPage(UIWidget w, bool instant)
	{
		w.gameObject.SetActive(value: true);
		if (instant)
		{
			w.SetEnable<TweenAlpha>(enable: false);
			w.alpha = 1f;
		}
		else
		{
			w.alpha = 0f;
			TweenAlpha.Begin(w.gameObject, 0.2f, 1f);
		}
	}
}
