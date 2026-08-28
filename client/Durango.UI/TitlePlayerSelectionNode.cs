using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Clusters;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class TitlePlayerSelectionNode : UIWidget
{
	[SerializeField]
	private SelectableWidget _button;

	[SerializeField]
	private UIWidget _profileWidget;

	[SerializeField]
	private UIWidget _deletedPlayerSymbol;

	[SerializeField]
	private UIWidget _newCharacterWidget;

	[SerializeField]
	private UIWidget _loadingRing;

	[SerializeField]
	private UIWidget _lockSlotWidget;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _freqLabel;

	[SerializeField]
	private UILabel _clanLabel;

	[SerializeField]
	private UIWidget _clanLabelDecorationWidget;

	[SerializeField]
	private UIWidget[] _clanInfoWidgets;

	[SerializeField]
	private UILabel _currentRegionLabel;

	[SerializeField]
	private UILabel _homeRegionLabel;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UISprite _clanOutlineSprite;

	[SerializeField]
	private Texture _portraitMask;

	[SerializeField]
	private GameObject _deleteInEditable;

	private List<UIWidget> _widgets;

	private UIWidget _previousState;

	private int _requestVersion;

	private string _boundPlayerEntityId;

	private void ActiveWidget(UIWidget target)
	{
		if (_widgets == null)
		{
			_widgets = new List<UIWidget>(new UIWidget[4] { _lockSlotWidget, _newCharacterWidget, _profileWidget, _loadingRing });
		}
		foreach (UIWidget widget in _widgets)
		{
			widget.gameObject.SetActive(widget == target);
		}
	}

	public void SetLocked()
	{
		ActiveWidget(_lockSlotWidget);
	}

	public void Set([CanBeNull] Durango.Logic.Clusters.PlayerInfo player, Action<Durango.Logic.Clusters.PlayerInfo> clicked, Action<Durango.Logic.Clusters.PlayerInfo> doubleClicked, Action<Durango.Logic.Clusters.PlayerInfo> deleteClicked = null)
	{
		int requestVersion = ++_requestVersion;
		_boundPlayerEntityId = (player == null) ? null : player.PlayerEntityId;
		_portraitTexture.mainTexture = null;
		MarkAsSoftDeleted(isDeleted: false);
		_button.Clicked = delegate
		{
			if (clicked != null)
			{
				clicked(player);
			}
		};
		_button.DoubleClicked = delegate
		{
			if (doubleClicked != null)
			{
				doubleClicked(player);
			}
		};
		// The delete action is shown in the bottom action bar by
		// TitlePlayerSelectionGroupBase, not inside each character card.
		_deleteInEditable.SetActive(value: false);
		UIEventListener.Get(_deleteInEditable).onClick = null;
		if (player == null)
		{
			ActiveWidget(_newCharacterWidget);
			return;
		}
		if (player.OfflineFunc != null)
		{
			Pair<PortraitBuilder.Argument, int> pair = player.OfflineFunc();
			ActiveWidget(_profileWidget);
			SetTextContent(player.PlayerLevel, player.PlayerName, Durango.Player.PlayerInfo.ToFreq(pair.Item2), string.Empty);
			MarkAsSoftDeleted(player.IsSoftDeleted);
			PortraitBuilder.Argument item = pair.Item1;
			item.Mask = _portraitMask;
			PortraitBuilder.Set(item, _portraitTexture);
			_currentRegionLabel.text = string.Empty;
			_homeRegionLabel.text = string.Empty;
			return;
		}
		ActiveWidget(_loadingRing);
		Singleton<PlayerInfoManager>.Instance().RequestNewPlayerInfo(player.PlayerEntityId, delegate(Durango.Player.PlayerInfo info)
		{
			if (requestVersion != _requestVersion || _boundPlayerEntityId != player.PlayerEntityId)
			{
				return;
			}
			if (info == null || !info.Valid)
			{
				ActiveWidget(_profileWidget);
				SetTextContent(player.PlayerLevel, player.PlayerName, string.Empty, string.Empty);
				MarkAsSoftDeleted(player.IsSoftDeleted);
				_currentRegionLabel.text = string.Empty;
				_homeRegionLabel.text = string.Empty;
				return;
			}
			ActiveWidget(_profileWidget);
			SetTextContent(info.Level, info.Name, info.GetFreq(20), info.ClanName);
			MarkAsSoftDeleted(player.IsSoftDeleted);
			PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
			portraitArgument.Mask = _portraitMask;
			PortraitBuilder.Set(portraitArgument, _portraitTexture);
			_currentRegionLabel.text = string.Format("{0} {1}", "icon_popup_player_island".ToEncodedIcon(), (info.Region == null) ? T._("알 수 없음") : info.RegionName);
			_homeRegionLabel.text = string.Format("{0} {1}", "icon_popup_player_house".ToEncodedIcon(), (info.ReturningRegion == null) ? T._("알 수 없음") : info.ReturningRegionName);
		});
	}

	private void SetTextContent(int level, string playerName, string freq, string clanName)
	{
		_nameLabel.text = playerName;
		_levelLabel.text = T._("{0:lv:}", level);
		_freqLabel.text = freq;
		bool flag = !string.IsNullOrEmpty(clanName);
		_clanLabel.text = string.Format("{0}", (!flag) ? T._("부족 없음") : clanName);
		_clanOutlineSprite.color = ((!flag) ? new Color(1f, 1f, 1f, 0.5f) : new Color(0.98f, 0.75f, 0.2f, 0.5f));
		int i = 0;
		for (int size = KUtility.GetSize(_clanInfoWidgets); i < size; i++)
		{
			UIWidget uIWidget = _clanInfoWidgets[i];
			uIWidget.color = ((!flag) ? new Color(1f, 1f, 1f, 0.35f) : PresetColor.UISunglowYellow);
		}
		Vector2 printedSize = _clanLabel.printedSize;
		int val = (int)printedSize.x + 50;
		_clanLabelDecorationWidget.SetDimensions(Math.Max(120, val), (!(printedSize.y > 21f)) ? 36 : 48);
	}

	public void MarkAsSoftDeleted(bool isDeleted)
	{
		_deletedPlayerSymbol.gameObject.SetActive(isDeleted);
	}

	public GameObject CreateDeleteButton(Transform parent)
	{
		if (_deleteInEditable == null || parent == null)
		{
			return null;
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(_deleteInEditable);
		gameObject.name = "BottomDeleteButton";
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localRotation = Quaternion.identity;
		return gameObject;
	}

	public void SetLoading(bool isLoading)
	{
		if (isLoading)
		{
			_previousState = _widgets.First((UIWidget elem) => elem.gameObject.activeSelf);
			ActiveWidget(_loadingRing);
		}
		else
		{
			ActiveWidget(_previousState);
		}
	}

	public void Clicked()
	{
		if (_button.Clicked != null)
		{
			_button.Clicked();
		}
	}
}
