using Durango.Logic.Notification;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PurchaseTabWidget : SelectableWidget
{
	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UISprite _notificationObject;

	[SerializeField]
	private RectLayout _layout;

	private void Start()
	{
		_textLabel.text = T._("보관함");
	}

	public void SetMode(bool isSimple)
	{
		_textLabel.gameObject.SetActive(!isSimple);
		_layout.UpdateLayout();
	}

	public void SetNotifiation(bool on)
	{
		_notificationObject.gameObject.SetActive(on);
		_notificationObject.color = Notification.GetTypeColor(Type.Important);
	}
}
