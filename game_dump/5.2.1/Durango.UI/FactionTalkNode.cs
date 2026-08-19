using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class FactionTalkNode : MonoBehaviour
{
	[SerializeField]
	private UISprite _portraitSprite;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMask;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private GameObject _separator;

	private UIWidget _widget;

	private const int MinHeight = 100;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public void Set(Shared.Faction.Messenger messenger, string message)
	{
		if (messenger == Shared.Faction.Messenger.Player)
		{
			PortraitBuilder.Argument portraitArgument = PlayerBehavior.LocalPlayer.GetPortraitArgument();
			portraitArgument.Mask = _portraitMask;
			PortraitBuilder.Set(portraitArgument, _portraitTexture);
			_titleLabel.text = PlayerBehavior.LocalPlayer.PlayerName;
			_portraitTexture.gameObject.SetActive(value: true);
			_portraitSprite.gameObject.SetActive(value: false);
		}
		else
		{
			Yaml.Messenger messenger2 = SingletonDict<Shared.Faction.Messenger, Yaml.Messenger>.Get(messenger);
			_portraitSprite.spriteName = messenger2.Portrait;
			_titleLabel.text = messenger2.Name;
			_portraitTexture.gameObject.SetActive(value: false);
			_portraitSprite.gameObject.SetActive(value: true);
		}
		_commentLabel.text = message;
		Widget.height = Mathf.Max(100, (int)_commentLabel.printedSize.y + 73);
		UIUtility.UpdateAnchors(base.transform);
	}

	public void SeparatorOn(bool on)
	{
		_separator.gameObject.SetActive(on);
	}
}
