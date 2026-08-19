using System.Collections.Generic;
using System.Text;
using Messages;
using Shared.Faction;
using UnityEngine;

public class FactionNoteContainer : FadeInOutContainer
{
	[SerializeField]
	private UILabel _textUpdatedTime;

	[SerializeField]
	private UILabel _textNote;

	[SerializeField]
	private FactionNoteButton _buttonPrevious;

	[SerializeField]
	private FactionNoteButton _buttonNext;

	[SerializeField]
	private Color _buttonColorNormal;

	[SerializeField]
	private Color _buttonColorPressed;

	[SerializeField]
	private Color _buttonColorDisabled;

	[SerializeField]
	private UIScrollView _scrollView;

	private FactionType _currentFaction = FactionType.Invalid;

	private int _currentIndex;

	private int _recordsCount;

	public override void Init()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		base.Init();
		UIEventListener.Get(((Component)_buttonPrevious).gameObject).onClick = OnClickPreviousButton;
		UIEventListener.Get(((Component)_buttonNext).gameObject).onClick = OnClickNextButton;
		_buttonPrevious.SetColors(_buttonColorNormal, _buttonColorPressed, _buttonColorDisabled);
		_buttonNext.SetColors(_buttonColorNormal, _buttonColorPressed, _buttonColorDisabled);
		UIUtility.SetScrollViewInvisibleBox(_scrollView);
	}

	public void Refresh(FactionType type, int index)
	{
		FactionSystem factionSystem = GameSystem<FactionSystem>.Instance();
		IList<FactionRadioRecord> factionRecords = factionSystem.GetFactionRecords(type);
		if (factionRecords != null && 0 <= index && index < factionRecords.Count)
		{
			FactionRadioRecord factionRadioRecord = factionRecords[index];
			_currentFaction = type;
			_currentIndex = index;
			_recordsCount = factionRecords.Count;
			_textUpdatedTime.text = TimerSystem.Timeago(factionRadioRecord.ReceivedAt);
			_textNote.text = GetMergedText(factionRadioRecord.Messages);
			_buttonPrevious.IsEnabled = index < factionRecords.Count - 1;
			_buttonNext.IsEnabled = 0 < index;
		}
		else
		{
			_currentFaction = FactionType.Invalid;
			_currentIndex = 0;
			_recordsCount = 0;
			_textUpdatedTime.text = string.Empty;
			_textNote.text = string.Empty;
			_buttonPrevious.IsEnabled = false;
			_buttonNext.IsEnabled = false;
		}
		_scrollView.ResetPosition();
	}

	private void OnClickPreviousButton(GameObject obj)
	{
		if (_currentFaction != FactionType.Invalid && _currentIndex < _recordsCount - 1)
		{
			Refresh(_currentFaction, _currentIndex + 1);
		}
	}

	private void OnClickNextButton(GameObject obj)
	{
		if (_currentFaction != FactionType.Invalid && 0 < _currentIndex)
		{
			Refresh(_currentFaction, _currentIndex - 1);
		}
	}

	private static string GetMergedText(string[] texts)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < texts.Length; i++)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine(texts[i]);
		}
		return stringBuilder.ToString();
	}
}
