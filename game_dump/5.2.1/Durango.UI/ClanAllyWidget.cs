using System;
using Durango.Logic.Clan;
using Durango.Network;
using Durango.UI.Control;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ClanAllyWidget : MonoBehaviour
{
	public Action<AllySlot> ButtonClicked;

	[SerializeField]
	private UITexture _emblemTexture;

	[SerializeField]
	private UITexture _noEmblem;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _memberLabel;

	[SerializeField]
	private UIWidget _timerWidget;

	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private SelectableButton _actionButton;

	[SerializeField]
	private RectLayout _layout;

	private UIWidget _widget;

	private AllySlot _slot;

	private bool _clanLoaded;

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

	private void Start()
	{
		_actionButton.Clicked = delegate
		{
			if (ButtonClicked != null)
			{
				ButtonClicked(_slot);
			}
		};
	}

	public void Set(AllySlot slot, bool hasPermission)
	{
		_slot = slot;
		_clanLoaded = false;
		ClanSystem.GetClanInfo(slot.ClanId, OnClan);
		if (!_clanLoaded)
		{
			_nameLabel.text = string.Empty;
			_levelLabel.text = string.Empty;
			_memberLabel.text = string.Empty;
		}
		_noEmblem.alpha = 0f;
		_emblemTexture.alpha = 0f;
		ClanSystem.GetEmblem(slot.ClanId, OnEmblem);
		_actionButton.Disabled = !hasPermission;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double stateExpiresAt = ((!_slot.StateExpiresAt.HasValue) ? 0.0 : _slot.StateExpiresAt.Value);
		if (stateExpiresAt > predictedServerTime)
		{
			_timerWidget.gameObject.SetActive(value: true);
			_timerLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				double num2 = stateExpiresAt - Connections.Frontend.GetPredictedServerTime();
				if (num2 > 0.0)
				{
					text = "[695D51][icon=icon_skill_time][-] " + TimedeltaFormatter.ColonFormat(num2);
					period = (float)(num2 % 1.0);
				}
				else
				{
					text = string.Empty;
					period = 0f;
				}
			}));
		}
		else if (_slot.AllySince.HasValue)
		{
			double allySince = _slot.AllySince.Value;
			_timerWidget.gameObject.SetActive(value: true);
			_timerLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				double num = Connections.Frontend.GetPredictedServerTime() - allySince;
				text = "[695D51][icon=icon_skill_time][-] " + TimedeltaFormatter.ColonFormat(num);
				period = (float)(num % 1.0);
			}));
		}
		else
		{
			_timerWidget.gameObject.SetActive(value: false);
			_timerLabel.text = string.Empty;
		}
		_layout.UpdateLayout();
	}

	private void OnEmblem(Point2 pos)
	{
		if (pos.x < 0 || pos.y < 0)
		{
			_noEmblem.alpha = 1f;
			_emblemTexture.alpha = 0f;
		}
		else
		{
			_noEmblem.alpha = 0f;
			_emblemTexture.alpha = 1f;
			EmblemTexture.Set(_emblemTexture, pos);
		}
	}

	private void OnClan(Clan clan)
	{
		_clanLoaded = true;
		_nameLabel.text = clan.Name;
		_levelLabel.text = $"[695D51][icon=icon_align_lv:1.2][-] {clan.Level}";
		_memberLabel.text = $"[695D51][icon=icon_person_big][-] {clan.MemberCount} [FFFFFF7F]/[-] {clan.Capacity}";
	}
}
