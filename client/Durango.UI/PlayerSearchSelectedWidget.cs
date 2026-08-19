using System;
using Durango.Player;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class PlayerSearchSelectedWidget : UIWidget
{
	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMaskTexture;

	[SerializeField]
	private UILabel _nameLabel;

	private string _entityId;

	public event Action Canceled;

	public string GetEntityId()
	{
		return _entityId;
	}

	public void Set(string entityId)
	{
		_entityId = entityId;
		PlayerInfo cachedPlayerInfoOrEmpty = Singleton<PlayerInfoManager>.Instance().GetCachedPlayerInfoOrEmpty(_entityId);
		if (!cachedPlayerInfoOrEmpty.Valid)
		{
			_portraitTexture.gameObject.SetActive(value: false);
			_nameLabel.gameObject.SetActive(value: false);
			return;
		}
		_portraitTexture.gameObject.SetActive(value: true);
		PortraitBuilder.Argument portraitArgument = cachedPlayerInfoOrEmpty.GetPortraitArgument();
		portraitArgument.Mask = _portraitMaskTexture;
		PortraitBuilder.Set(portraitArgument, _portraitTexture);
		_nameLabel.gameObject.SetActive(value: true);
		_nameLabel.text = cachedPlayerInfoOrEmpty.Name;
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (this.Canceled != null)
		{
			this.Canceled();
		}
	}
}
