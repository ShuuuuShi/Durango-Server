using System;
using Durango.Logic.Social;
using Durango.UI.Control;
using Durango.UI.Popup;
using UnityEngine;

namespace Durango.UI;

public class EmotionSelector : TooltipBase
{
	public const int FavoriteIconSize = 40;

	public Action TooltipOpened;

	public Action TooltipClosed;

	[SerializeField]
	private UIWidget _parentTarget;

	[SerializeField]
	private EmotionQuickslotWidget _quickslotWidget;

	[SerializeField]
	private SelectableWidget _favoritesModifyButton;

	[SerializeField]
	private SelectableWidget _penButton;

	[SerializeField]
	private UISprite _emotionNewSprite;

	[SerializeField]
	private SelectableWidget _modificationGuideButton;

	[SerializeField]
	private GameObject _modificationGuideObject;

	public static bool CanModify => GameManager.Region.IsAfterSafeHouse();

	private static bool IsGuideSeen
	{
		get
		{
			return Preferences.GetBool("emotion_selector_guide_watched");
		}
		set
		{
			Preferences.SetBool("emotion_selector_guide_watched", value);
		}
	}

	public event Action PenClicked
	{
		add
		{
			SelectableWidget penButton = _penButton;
			penButton.Clicked = (Action)Delegate.Combine(penButton.Clicked, value);
		}
		remove
		{
			SelectableWidget penButton = _penButton;
			penButton.Clicked = (Action)Delegate.Remove(penButton.Clicked, value);
		}
	}

	public event Action<Emoticon> EmoticonClicked
	{
		add
		{
			_quickslotWidget.EmoticonClicked += value;
		}
		remove
		{
			_quickslotWidget.EmoticonClicked -= value;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		SoundType = UISound.GroupType.NoSound;
		_favoritesModifyButton.Clicked = delegate
		{
			EmotionFavoritesGroup emotionFavoritesGroup = UIManager.FindScript<EmotionFavoritesGroup>();
			if (emotionFavoritesGroup != null)
			{
				base.HideIgnoreParent = emotionFavoritesGroup.transform;
				emotionFavoritesGroup.Open();
			}
		};
		_modificationGuideButton.Clicked = delegate
		{
			IsGuideSeen = true;
			_modificationGuideObject.SetActive(value: false);
		};
		GameSystem<SocialSystem>.Instance().Emotional.Changed += delegate
		{
			_quickslotWidget.Refersh();
		};
		GameSystem<SocialSystem>.Instance().Emotional.EmoticonNotification.Changed += UpdateEmotionNotifiaction;
		GameSystem<SocialSystem>.Instance().Emotional.MotionNotification.Changed += UpdateEmotionNotifiaction;
	}

	protected override void FillData()
	{
		base.FillData();
		_quickslotWidget.Refersh();
		UpdateEmotionNotifiaction();
		_favoritesModifyButton.gameObject.SetActive(CanModify);
		_modificationGuideObject.SetActive(CanModify && !IsGuideSeen);
	}

	protected override void UpdateLayout()
	{
		UIWidget rootAnchor = UIRootAnchor.GetRootAnchor(UIBase.AnchorType.Base);
		Vector3 vector = rootAnchor.worldCorners[1];
		Vector3 position = new Vector3(vector.x, _parentTarget.worldCorners[1].y, 0f);
		position = base.transform.parent.InverseTransformPoint(position);
		position.x += 13f + vector.x;
		base.transform.localPosition = position;
	}

	public EmoticonWidget FindNode(string key)
	{
		return _quickslotWidget.FindEmoticonWidget(key);
	}

	private void UpdateEmotionNotifiaction()
	{
		bool hasNewNotifiaction = GameSystem<SocialSystem>.Instance().Emotional.HasNewNotifiaction;
		_emotionNewSprite.gameObject.SetActive(hasNewNotifiaction);
	}

	protected override void OnShow()
	{
		base.OnShow();
		if (TooltipOpened != null)
		{
			TooltipOpened();
		}
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (TooltipClosed != null)
		{
			TooltipClosed();
		}
	}
}
