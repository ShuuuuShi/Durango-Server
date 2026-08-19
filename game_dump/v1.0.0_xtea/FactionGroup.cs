using System.Collections.Generic;
using Messages;
using Shared.Faction;
using UnityEngine;

public class FactionGroup : UIBase
{
	private enum Mode
	{
		Summary,
		History,
		Note
	}

	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private FactionSummaryContainer _summaryContainer;

	[SerializeField]
	private FactionHistoryContainer _historyContainer;

	[SerializeField]
	private FactionNoteContainer _factionNoteContainer;

	private Mode _currentMode;

	private void Awake()
	{
		_summaryContainer.Init();
		_historyContainer.Init();
		_factionNoteContainer.Init();
		_summaryContainer.SummaryClicked += _summaryContainer_SummaryClicked;
		_historyContainer.FactionCommListItemClicked += HistoryContainerFactionCommListItemClicked;
		SetMode(Mode.Summary);
		OnClose();
	}

	private void Start()
	{
		_titleWidget.OnClose += _titleWidget_OnClose;
		_titleWidget.OnBack += _titleWidget_OnBack;
	}

	private void OnEnable()
	{
		GameSystem<FactionSystem>.Instance().FactionsUpdated += FactionSystemFactionsUpdated;
	}

	private void OnDisable()
	{
		GameSystem<FactionSystem>.Instance().FactionsUpdated -= FactionSystemFactionsUpdated;
	}

	protected override bool OnOpen()
	{
		GameSystem<FactionSystem>.Instance().RequestFactions();
		SetMode(Mode.Summary, instant: true);
		return base.OnOpen();
	}

	private void SetMode(Mode mode, bool instant = false)
	{
		_titleWidget.ShowBackButton(mode != Mode.Summary, instant);
		_summaryContainer.Show(mode == Mode.Summary, instant);
		_historyContainer.Show(mode == Mode.History, instant);
		_factionNoteContainer.Show(mode == Mode.Note, instant);
		_currentMode = mode;
	}

	private void _titleWidget_OnClose()
	{
		_titleWidget.ShowBackButton(isShow: false, instant: true);
		((Component)_summaryContainer).gameObject.SetActive(false);
		((Component)_historyContainer).gameObject.SetActive(false);
		((Component)_factionNoteContainer).gameObject.SetActive(false);
		ForceClose();
	}

	private void _titleWidget_OnBack()
	{
		switch (_currentMode)
		{
		case Mode.History:
			SetMode(Mode.Summary);
			break;
		case Mode.Note:
			SetMode(Mode.History);
			break;
		}
	}

	private void _summaryContainer_SummaryClicked(FactionType type)
	{
		SetMode(Mode.History);
		_historyContainer.SetSelection(type);
	}

	private void HistoryContainerFactionCommListItemClicked(FactionType type, int index)
	{
		SetMode(Mode.Note);
		_factionNoteContainer.Refresh(type, index);
	}

	private void FactionSystemFactionsUpdated(IList<Faction?> factions)
	{
		_summaryContainer.Refresh(factions);
		_historyContainer.Refresh(factions);
	}
}
