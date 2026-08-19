using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class PetMilestoneHelpPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private PetMilestoneHelpTabWidget _tabWidget;

	[SerializeField]
	private PetMilestoneHelpTipPage _tipPage;

	[SerializeField]
	private PetMilestoneHelpPage _infoPage;

	private Pet _pet;

	private bool _isGraphView;

	private readonly List<MilestoneCandidates?> _milestoneCandidateses = new List<MilestoneCandidates?>();

	private List<Pair<PetActiveSkill, float>> _activeSkillCandidates;

	private bool _isTipPage;

	private bool _isActiveSkillPage;

	private int? _isMilestonePage;

	private uint _sequence;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		PetMilestoneHelpTabWidget tabWidget = _tabWidget;
		tabWidget.TipClicked = (Action)Delegate.Combine(tabWidget.TipClicked, new Action(ShowTipPage));
		PetMilestoneHelpTabWidget tabWidget2 = _tabWidget;
		tabWidget2.ActiveSkillClicked = (Action)Delegate.Combine(tabWidget2.ActiveSkillClicked, new Action(ShowActiveSkillPage));
		PetMilestoneHelpTabWidget tabWidget3 = _tabWidget;
		tabWidget3.MilestoneClicked = (Action<MilestoneInfo>)Delegate.Combine(tabWidget3.MilestoneClicked, new Action<MilestoneInfo>(ShowMilestonePage));
		_infoPage.GraphButtonClicked += delegate
		{
			_isGraphView = !_isGraphView;
			RefreshPage();
		};
	}

	public void Set(Pet pet)
	{
		_pet = pet;
		_activeSkillCandidates = null;
		_milestoneCandidateses.Clear();
	}

	protected override void FillData()
	{
		_titleLabel.text = T._("{0}의 성장 정보", _pet.GetPetName());
		_tabWidget.Set(_pet);
	}

	protected override void UpdateLayout()
	{
		RectLayoutComponent component = GetComponent<RectLayoutComponent>();
		component.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	protected override void OnShow()
	{
		base.OnShow();
		_isGraphView = false;
		ShowTipPage();
	}

	private void RefreshPage()
	{
		if (_isTipPage)
		{
			_tabWidget.SelectTip();
			_tipPage.gameObject.SetActive(value: true);
			_infoPage.gameObject.SetActive(value: false);
		}
		else if (_isActiveSkillPage)
		{
			_tabWidget.SelectActiveSkill();
			_tipPage.gameObject.SetActive(value: false);
			_infoPage.gameObject.SetActive(value: true);
			_infoPage.ShowTitle(T._("특수 행동"));
			if (_activeSkillCandidates == null)
			{
				_activeSkillCandidates = PetUtil.GetActiveSkillCandidates(_pet);
			}
			_infoPage.ShowActiveSkillCandidates(_activeSkillCandidates, _isGraphView, instant: true);
		}
		else
		{
			if (!_isMilestonePage.HasValue)
			{
				return;
			}
			int index = _isMilestonePage.Value;
			_tabWidget.SelectMilestone(index);
			_tipPage.gameObject.SetActive(value: false);
			_infoPage.gameObject.SetActive(value: true);
			if (_pet.Statistics.MilestonesInformation == null)
			{
				return;
			}
			MilestoneInfo[] milestonesInformation = _pet.Statistics.MilestonesInformation;
			if (!milestonesInformation.TryGet(index, out var milestone))
			{
				return;
			}
			_infoPage.ShowTitle(T._("{0} 에 발견할 수 있는 속성", LocalizeUtil.FormatLevel(milestone.Level)));
			if (milestone.Acquired)
			{
				_infoPage.ShowAcquiredMilestone(milestone, instant: true);
				return;
			}
			MilestoneCandidates? milestoneCandidates = _milestoneCandidateses.Get(index);
			if (!milestoneCandidates.HasValue)
			{
				_infoPage.ShowEmpty(_isGraphView);
				uint seq = ++_sequence;
				PetManager.GetMilestoneCandidate(_pet.EntityId, milestone.MilestoneTableId, delegate(MilestoneCandidates? candidates)
				{
					while (_milestoneCandidateses.Count <= index)
					{
						_milestoneCandidateses.Add(null);
					}
					_milestoneCandidateses[index] = candidates;
					if (_sequence == seq && candidates.HasValue)
					{
						_infoPage.ShowMilestoneCandidates(milestone, candidates.Value, _isGraphView, instant: false);
					}
				});
			}
			else
			{
				_infoPage.ShowMilestoneCandidates(milestone, milestoneCandidates.Value, _isGraphView, instant: true);
			}
		}
	}

	private void ShowTipPage()
	{
		_isTipPage = true;
		_isActiveSkillPage = false;
		_isMilestonePage = null;
		RefreshPage();
	}

	private void ShowActiveSkillPage()
	{
		_isTipPage = false;
		_isActiveSkillPage = true;
		_isMilestonePage = null;
		RefreshPage();
	}

	private void ShowMilestonePage(MilestoneInfo milestone)
	{
		_isTipPage = false;
		_isActiveSkillPage = false;
		_isMilestonePage = null;
		MilestoneInfo[] milestonesInformation = _pet.Statistics.MilestonesInformation;
		if (milestonesInformation != null)
		{
			int num = milestonesInformation.IndexOf((MilestoneInfo info) => info.MilestoneTableId == milestone.MilestoneTableId);
			if (num != -1)
			{
				_isMilestonePage = num;
				RefreshPage();
			}
		}
	}
}
