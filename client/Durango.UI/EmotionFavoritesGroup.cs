using Durango.Logic.Notification;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

[Uri("Emotion")]
public class EmotionFavoritesGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private NestedPrefabLinker _categories;

	[SerializeField]
	private PageMotions _pageMotions;

	[SerializeField]
	private PageEmoticons _pageEmoticons;

	private IconTabList _tabList;

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("즐겨찾기"));
		GameSystem<SocialSystem>.Instance().Emotional.Changed += Emotional_Changed;
		GameSystem<SocialSystem>.Instance().Emotional.MotionNotification.Changed += UpdateEmotionNotification;
		GameSystem<SocialSystem>.Instance().Emotional.EmoticonNotification.Changed += UpdateEmoticonNotification;
		base.OnOpenSucceed += delegate
		{
			SelectTab(0);
		};
		InitializeTabList();
		SetChildrenActive(activated: false);
	}

	private void UpdateEmotionNotification()
	{
		Container motionNotification = GameSystem<SocialSystem>.Instance().Emotional.MotionNotification;
		_tabList.SetNotification(0, motionNotification.On, motionNotification.Type);
	}

	private void UpdateEmoticonNotification()
	{
		Container emoticonNotification = GameSystem<SocialSystem>.Instance().Emotional.EmoticonNotification;
		_tabList.SetNotification(1, emoticonNotification.On, emoticonNotification.Type);
	}

	private void InitializeTabList()
	{
		_tabList = _categories.Object.GetComponent<IconTabList>();
		_tabList.BeginLoad();
		_tabList.Add(null, T._("감정표현"));
		_tabList.Add(null, T._("이모티콘"));
		_tabList.EndLoad();
		_tabList.Clicked += SelectTab;
		UpdateEmotionNotification();
		UpdateEmoticonNotification();
	}

	private void SelectTab(int index)
	{
		bool flag = index == 0;
		bool flag2 = index == 1;
		_pageMotions.gameObject.SetActive(flag);
		_pageEmoticons.gameObject.SetActive(flag2);
		_tabList.Select(index);
		if (flag)
		{
			_pageMotions.Refresh();
		}
		if (flag2)
		{
			_pageEmoticons.Refresh();
		}
	}

	private void Emotional_Changed()
	{
		if (base.IsOpened)
		{
			_pageEmoticons.Refresh(reset: false);
			_pageMotions.Refresh(reset: false);
		}
	}

	protected override bool TryClose()
	{
		if (GameSystem<SocialSystem>.HasInstance())
		{
			GameSystem<SocialSystem>.Instance().Emotional.SaveFavorites();
		}
		return base.TryClose();
	}

	[Uri("MotionPreview")]
	private void ShowMotionPreviewPopup(string motion)
	{
		MotionPreviewPopup motionPreviewPopup = UIManager.Popup.Tooltip<MotionPreviewPopup>();
		motionPreviewPopup.Set(motion);
		motionPreviewPopup.Show();
	}
}
