using System;
using Durango.Logic.Statistics;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class CharacterTitleSelectorItem : UIWidget
{
	[SerializeField]
	private UISprite _checkSprite;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UIEventListener _button;

	[SerializeField]
	private UIEventListener _favoriteButton;

	[SerializeField]
	private UISprite _favoriteButtonMark;

	[SerializeField]
	private RectLayout _layout;

	public Title TargetTitle { get; private set; }

	public event Action<CharacterTitleSelectorItem> Clicked;

	public event Action<CharacterTitleSelectorItem> FavoriteClicked;

	protected override void OnStart()
	{
		base.OnStart();
		_button.onClick = ClickBodyButton;
		_favoriteButton.onClick = ClickFavoriteButton;
	}

	public void Set(Title targetTitle, bool isSelected, bool isFavorite)
	{
		TargetTitle = targetTitle;
		if (TargetTitle == null)
		{
			_textLabel.text = string.Format("{0}\n[size=4] [/size]\n[size=20][777163]{1}", T._("칭호 없음"), T._("진로 가이드를 완료해 칭호를 획득해 보세요!"));
		}
		else
		{
			_textLabel.text = string.Format("{0}\n[size=4] [/size]\n[size=20][777163]{1}[-][/size]\n[size=9] [/size]\n[size=22][FFFFFF7F]{2}", TargetTitle.Name, TargetTitle.Description, T._("{0:l:{}|, }", TargetTitle.GetAbilityModifiersText()));
		}
		base.height = Mathf.Max(120, _textLabel.height + 40);
		_layout.UpdateLayout(base.width, base.height);
		UIUtility.UpdateAnchors(base.transform);
		_favoriteButton.gameObject.SetActive(targetTitle != null);
		_checkSprite.gameObject.SetActive(isSelected);
		_favoriteButtonMark.gameObject.SetActive(isFavorite);
	}

	private void ClickBodyButton(GameObject obj)
	{
		if (this.Clicked != null)
		{
			this.Clicked(this);
		}
	}

	private void ClickFavoriteButton(GameObject obj)
	{
		if (TargetTitle != null && this.FavoriteClicked != null)
		{
			this.FavoriteClicked(this);
		}
	}
}
