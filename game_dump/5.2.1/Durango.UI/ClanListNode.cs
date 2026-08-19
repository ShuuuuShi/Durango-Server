using System;
using Durango.Logic.Clan;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ClanListNode : SelectableWidget
{
	public Action<Clan> ButtonClicked;

	[SerializeField]
	private UITexture _emblemSprite;

	[SerializeField]
	private GameObject _noEmblem;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _numberLabel;

	[SerializeField]
	private SelectableButton _button;

	[SerializeField]
	private UILabel _waitingLabel;

	[SerializeField]
	private RectLayout _layout;

	[SerializeField]
	private GameObject[] _onlyLandcapeColumn;

	[SerializeField]
	private bool _isWaitingNode;

	private bool _isPortrait;

	private Point2 _size;

	private bool _hasButtonLayout;

	private string _clanId;

	[CanBeNull]
	public Clan Clan { get; private set; }

	private void Start()
	{
		_button.Clicked = delegate
		{
			if (ButtonClicked != null)
			{
				ButtonClicked(Clan);
			}
		};
		UIEventListener uIEventListener = UIEventListener.Get(_waitingLabel.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnWaitingLabelClicked));
	}

	public void Set(string id, string buttonText)
	{
		_clanId = id;
		bool flag = !string.IsNullOrEmpty(buttonText);
		if (flag)
		{
			Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
			bool flag2 = playerClan == null && GameSystem<ClanSystem>.Instance().WaitingClan != null && GameSystem<ClanSystem>.Instance().WaitingClan.Id == id;
			if (!_isWaitingNode && flag2)
			{
				_button.gameObject.SetActive(value: false);
				_waitingLabel.gameObject.SetActive(value: true);
			}
			else
			{
				bool flag3 = playerClan != null && playerClan.Id == id;
				_button.Text = buttonText;
				_button.Disabled = flag3;
				_button.Widget.alpha = ((!flag3) ? 1f : 0f);
				_button.gameObject.SetActive(value: true);
				_waitingLabel.gameObject.SetActive(value: false);
			}
		}
		else
		{
			_button.gameObject.SetActive(value: false);
			_waitingLabel.gameObject.SetActive(value: false);
		}
		bool flag4 = false;
		if (_hasButtonLayout != flag)
		{
			_hasButtonLayout = flag;
			flag4 = true;
		}
		Point2 point = new Point2(base.Widget.localSize);
		if (point != _size)
		{
			_size = point;
			flag4 = true;
		}
		bool flag5 = UIManager.IsPortraitWidget(base.gameObject);
		if (_isPortrait != flag5)
		{
			flag4 = true;
			_isPortrait = flag5;
			int i = 0;
			for (int size = KUtility.GetSize(_onlyLandcapeColumn); i < size; i++)
			{
				_onlyLandcapeColumn[i].SetActive(!_isPortrait);
			}
		}
		if (flag4)
		{
			_layout.UpdateLayout(point.x, point.y);
			UIUtility.UpdateAnchors(base.transform);
		}
		_noEmblem.gameObject.SetActive(value: false);
		_emblemSprite.gameObject.SetActive(value: false);
		ClanSystem.GetEmblem(_clanId, delegate(Point2 pos)
		{
			if (!(id != _clanId))
			{
				if (pos.x < 0 || pos.y < 0)
				{
					_noEmblem.gameObject.SetActive(value: true);
					_emblemSprite.gameObject.SetActive(value: false);
				}
				else
				{
					_noEmblem.gameObject.SetActive(value: false);
					_emblemSprite.gameObject.SetActive(value: true);
					EmblemTexture.Set(_emblemSprite, pos);
				}
			}
		});
		ClanSystem.GetClanInfo(_clanId, SetClan, refresh: false, detail: false);
		if (Clan == null || !(Clan.Id == _clanId))
		{
			_nameLabel.text = string.Empty;
			_levelLabel.text = string.Empty;
			_numberLabel.text = string.Empty;
		}
	}

	private void SetClan(Clan clan)
	{
		if (!(_clanId != clan.Id))
		{
			Clan = clan;
			_nameLabel.text = clan.Name;
			_levelLabel.text = LocalizeUtil.FormatLevel(clan.Level);
			_numberLabel.text = $"{clan.MemberCount} / {clan.Capacity}";
		}
	}

	private void OnWaitingLabelClicked(GameObject go)
	{
		UIManager.SystemMsg(T._("가입 대기 중입니다."));
	}
}
