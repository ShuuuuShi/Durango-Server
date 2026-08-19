using System;
using L10N;
using Player;
using TimerData;
using UnityEngine;

public class SelectablePlayerWidget : MonoBehaviour
{
	public Action<PlayerInfo> PlayerSelected;

	[SerializeField]
	private UIWidget _profileWiget;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _clanLabel;

	[SerializeField]
	private UISpriteLabel _freqLabel;

	[SerializeField]
	private UILabel _regionLabel;

	[SerializeField]
	private UISpriteLabel _connectLabel;

	[SerializeField]
	private UISprite _background;

	private PlayerInfo _player;

	private Vector3 _defaultNamelabelPos;

	private UIWidget _widget;

	private bool _isInit;

	public UIWidget Widget => (!((Object)(object)_widget != (Object)null)) ? (_widget = ((Component)this).GetComponent<UIWidget>()) : _widget;

	public bool IsSelect { get; private set; }

	private void Init()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_defaultNamelabelPos = ((Component)_nameLabel).transform.localPosition;
		}
	}

	public void Set(PlayerInfo player)
	{
		Init();
		_player = player;
		string name = player.Name;
		_nameLabel.text = name;
		((Component)_profileWiget).gameObject.SetActive(false);
		((Component)_clanLabel).gameObject.SetActive(false);
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(player.EntityId, DetailSet);
	}

	private void DetailSet(PlayerInfo player)
	{
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		_player = player;
		((Component)_profileWiget).gameObject.SetActive(true);
		((Component)_clanLabel).gameObject.SetActive(true);
		string text = (player.Valid ? player.Name : T._("알수없음"));
		string text2 = ((!string.IsNullOrEmpty(player?.ClanName)) ? player.ClanName : string.Empty);
		_nameLabel.text = text;
		_freqLabel.text = (player.Valid ? $"[icon_radio] {player.Freq:0000}" : "[icon_radio] ????");
		_regionLabel.text = (player.Valid ? player.Region.Name : string.Empty);
		if (!player.Valid)
		{
			_connectLabel.text = string.Empty;
		}
		else if (player.Online)
		{
			_connectLabel.text = string.Format("[icon_connect] {0}[-]", T._("접속 중"));
			_connectLabel.Label.color = UIManager.UIYellow;
		}
		else
		{
			double time = Connections.Frontend.GetPredictedServerTime() - player.DisconnectedAt;
			_connectLabel.Label.color = UIManager.UILightGray;
			string text3 = TimerSystem.TimeToString(time, TimePeriod.Min, 1);
			text3 = ((!string.IsNullOrEmpty(text3)) ? string.Format("{0}", LocalizeSystem.Format("#timeago_past", text3)) : T._("방금"));
			_connectLabel.text = text3;
		}
		if (!player.Valid)
		{
			((Component)_portraitTexture).gameObject.SetActive(false);
		}
		else
		{
			((Component)_portraitTexture).gameObject.SetActive(true);
			PortraitBuilder.Argument portraitArgument = player.GetPortraitArgument();
			PortraitBuilder.Set(portraitArgument, _portraitTexture);
		}
		if (string.IsNullOrEmpty(text2))
		{
			Vector3 defaultNamelabelPos = _defaultNamelabelPos;
			Vector2 pivotOffset = Widget.pivotOffset;
			defaultNamelabelPos.y = (float)Widget.height * (0.5f - pivotOffset.y);
			((Component)_nameLabel).transform.localPosition = defaultNamelabelPos;
			((Component)_clanLabel).gameObject.SetActive(false);
		}
		else
		{
			((Component)_nameLabel).transform.localPosition = _defaultNamelabelPos;
			_clanLabel.text = text2;
			((Component)_clanLabel).gameObject.SetActive(true);
		}
		UIUtility.UpdateAnchors(((Component)this).transform);
	}

	public int GetHeight()
	{
		return Widget.height;
	}

	private void OnClick()
	{
		if (PlayerSelected != null)
		{
			PlayerSelected(_player);
		}
	}

	public void Select(bool select)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((!select) ? Color.white : UIManager.UIYellow);
		color.a = _background.alpha;
		_background.color = color;
		IsSelect = select;
	}
}
