using System;
using L10N;
using MapData;
using Messages;
using Player;
using Shared.MessageBoard;
using UnityEngine;

public class MarketInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UITexture _textureWidget;

	[SerializeField]
	private GameObject _noEmblem;

	[SerializeField]
	private UILabel _regionLabel;

	[SerializeField]
	private UISpriteLabel _positionLabel;

	[SerializeField]
	private UILabel _commentLabel;

	[SerializeField]
	private DefaultSelectableButton _button;

	private Vector2 _tilePos;

	private ulong _sellerId;

	private string _positionFormat;

	private string _buttonFormat;

	private Texture2D _image;

	private bool _isInit;

	private void Init()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		if (!_isInit)
		{
			_isInit = true;
			_image = new Texture2D(0, 0);
			((Texture)_image).filterMode = (FilterMode)0;
			((Texture)_image).wrapMode = (TextureWrapMode)1;
			_positionFormat = _positionLabel.text;
			_buttonFormat = _button.Text;
			DefaultSelectableButton button = _button;
			button.Clicked = (Action)Delegate.Combine(button.Clicked, new Action(OnClickButton));
		}
	}

	public void Set(Market market)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		Init();
		bool flag = false;
		if (market.Scribble.HasValue && market.Scribble.Value.Type == Drawing.Canvas && market.Scribble.Value.Data != null)
		{
			flag = true;
			_image.LoadImage(market.Scribble.Value.Data);
		}
		if (flag)
		{
			((Component)_textureWidget).gameObject.SetActive(true);
			_textureWidget.mainTexture = (Texture)(object)_image;
			_noEmblem.gameObject.SetActive(false);
		}
		else
		{
			((Component)_textureWidget).gameObject.SetActive(false);
			_noEmblem.gameObject.SetActive(true);
		}
		_tilePos = market.Tile.ToVector2();
		_sellerId = market.SellerId;
		_regionLabel.text = market.Name;
		Vector2 val = MapPositionParser.PositionToHumaneTile(TerrainA6.TilePositionToWorldPosition(_tilePos));
		_positionLabel.text = T._(_positionFormat, MapPositionParser.ToString((int)val.x, (int)val.y));
		_commentLabel.text = string.Empty;
		_button.Text = T._(_buttonFormat, market.Name);
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(_sellerId, OnResponseSellerInfo, useOldCache: true);
	}

	private void OnResponseSellerInfo(Player.PlayerInfo playerInfo)
	{
		if (playerInfo.Valid)
		{
			_commentLabel.text = T._("어서오세요 {0}의 개인 가판대 입니다.", playerInfo.Name);
		}
		else
		{
			_commentLabel.text = string.Empty;
		}
	}

	private void OnClickButton()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		UIManager.FindScript<WorldMapGroup>().OpenForAnnounceBalloon(AnnounceType.Market, _tilePos, _sellerId);
	}
}
