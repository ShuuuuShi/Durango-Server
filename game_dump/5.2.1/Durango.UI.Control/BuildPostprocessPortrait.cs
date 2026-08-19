using System;
using Durango.Player;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Control;

public class BuildPostprocessPortrait : MonoBehaviour
{
	public Action<BuildPostprocessPortrait> Clicked;

	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private UISprite _bgSprite;

	[SerializeField]
	private Texture _textureMask;

	public PlayerInfo Player { get; private set; }

	public void Set(string entityId)
	{
		Player = PlayerInfoManager.EmptyPlayer;
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, OnPlayer);
		if (!Player.Valid)
		{
			_texture.alpha = 0f;
			_bgSprite.alpha = 0f;
		}
	}

	private void OnPlayer(PlayerInfo info)
	{
		_texture.alpha = 1f;
		_bgSprite.alpha = 1f;
		PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
		portraitArgument.Mask = _textureMask;
		_bgSprite.color = portraitArgument.BgColor;
		PortraitBuilder.Set(portraitArgument, _texture);
		Player = info;
	}

	private void OnClick()
	{
		if (Clicked != null)
		{
			Clicked(this);
		}
	}
}
