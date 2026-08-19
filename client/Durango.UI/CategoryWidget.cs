using Crafting;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class CategoryWidget : SelectableWidget
{
	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UISprite _notificationObject;

	[CanBeNull]
	public string Id => (Category == null) ? null : Category.Id;

	[CanBeNull]
	public Category Category { get; private set; }

	public void SetEntireCategory()
	{
		if (Category != null)
		{
			Category.Notification.Changed -= OnChangeNewState;
		}
		Category = null;
		_textLabel.text = T._("전체");
		_iconSprite.spriteName = "icon_search_big";
	}

	public void SetCategory([NotNull] Category category)
	{
		if (Category != null)
		{
			Category.Notification.Changed -= OnChangeNewState;
		}
		Category = category;
		SetNotification(Category.Notification.On);
		_textLabel.text = category.Name;
		_iconSprite.spriteName = IconMap.Get(category.Id, "icon_question");
		category.Notification.Changed += OnChangeNewState;
	}

	public void SetNotification(bool on)
	{
		_notificationObject.gameObject.SetActive(on);
	}

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
	}

	private void OnChangeNewState()
	{
		SetNotification(Category != null && Category.Notification.On);
	}
}
