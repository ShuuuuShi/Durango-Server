using EncyclopediaData;
using UnityEngine;

public class EncyclopediaGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private EncyclopediaMemoPage _memoPage;

	private void Start()
	{
		_titleWidget.OnClose += base.ForceClose;
		_titleWidget.OnBack += Close;
		_memoPage.OnShowMemo += delegate(bool show)
		{
			_titleWidget.ShowBackButton(show);
		};
		base.OnClose();
	}

	private void OnEnable()
	{
		GameSystem<EncyclopediaSystem>.Instance().MemoCollected += OnMemoCollect;
	}

	private void OnDisable()
	{
		GameSystem<EncyclopediaSystem>.Instance().MemoCollected -= OnMemoCollect;
	}

	public void Open(MemoType type, int index = -1)
	{
		Open();
		if (index == -1)
		{
			_memoPage.ShowMemoList(type);
		}
		else
		{
			_memoPage.ShowMemo(type, index);
		}
	}

	protected override bool OnOpen()
	{
		bool result = base.OnOpen();
		_memoPage.ShowMemoList(MemoType.Fiction);
		_titleWidget.ShowBackButton(isShow: false, instant: true);
		return result;
	}

	protected override bool OnClose()
	{
		if (!_memoPage.Close())
		{
			return false;
		}
		return base.OnClose();
	}

	private void OnMemoCollect(MemoType type, int index)
	{
		switch (type)
		{
		case MemoType.Survival:
		case MemoType.Submemo:
		{
			string memoTitle = EncyclopediaSystem.GetMemoTitle(type, index);
			string memoText = EncyclopediaSystem.GetMemoText(type, index);
			if (!string.IsNullOrEmpty(memoText))
			{
				string arg = ((20 >= memoText.Length) ? memoText : (memoText.Substring(0, 20) + "..."));
				UIManager.Popup.Alarm.ShowAlarm($"{memoTitle}: {arg}", "alarm_memo", 5f, delegate
				{
					Open(type, index);
				});
			}
			break;
		}
		}
	}
}
