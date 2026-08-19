using L10N;
using Player;
using TimerData;
using UnityEngine;

public class SocialPlayerCardWidget : MonoBehaviour
{
	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMask;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISpriteLabel _freqLabel;

	[SerializeField]
	private UILabel _locLabel;

	[SerializeField]
	private UISpriteLabel _connectionLabel;

	[SerializeField]
	private GameObject _followerMaker;

	private AnimationWidget _animWidget;

	private bool _isActivate;

	public AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public PlayerInfo PlayerInfo { get; private set; }

	public bool IsInitPosition { get; set; }

	public bool Activate
	{
		get
		{
			return _isActivate;
		}
		set
		{
			_isActivate = value;
			AnimWidget.Alpha = ((!value) ? 0f : 1f);
		}
	}

	private void OnEnable()
	{
		GameSystem<SocialSystem>.Instance().FollowingListUpdated += UpdateFollowMacker;
	}

	private void OnDisable()
	{
		GameSystem<SocialSystem>.Instance().FollowingListUpdated -= UpdateFollowMacker;
		AnimWidget.SetAlpha(0f, useTween: false);
	}

	private void OnClick()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerInfo != null)
		{
			ProfileTooltip profileTooltip = UIManager.Popup.Tooltip<ProfileTooltip>();
			profileTooltip.Set(PlayerInfo);
			profileTooltip.Show(AnimWidget.Widget, Vector2.zero, 3600f);
		}
	}

	public void Set(PlayerInfo playerInfo)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		PlayerInfo = playerInfo;
		PortraitBuilder.Argument portraitArgument = playerInfo.GetPortraitArgument();
		portraitArgument.Mask = _portraitMask;
		PortraitBuilder.Set(portraitArgument, _portraitTexture);
		_levelLabel.text = playerInfo.Level.ToString();
		_nameLabel.text = playerInfo.Name;
		_freqLabel.text = $"[icon_radio] {playerInfo.Freq:0000}";
		_locLabel.text = $"{playerInfo.Region.Name}";
		if (playerInfo.Online)
		{
			_connectionLabel.text = string.Format("[icon_connect] {0}[-]", T._("접속 중"));
			_connectionLabel.Label.color = UIManager.UIBlack;
		}
		else
		{
			double time = Connections.Frontend.GetPredictedServerTime() - playerInfo.DisconnectedAt;
			_connectionLabel.Label.color = UIManager.UILightGray;
			string text = TimerSystem.TimeToString(time, TimePeriod.Min, 1);
			text = ((!string.IsNullOrEmpty(text)) ? T._("{0} 전", text) : T._("방금"));
			_connectionLabel.text = text;
		}
		UpdateFollowMacker();
	}

	private void UpdateFollowMacker()
	{
		_followerMaker.gameObject.SetActive(GameSystem<SocialSystem>.Instance().FollowingList.Contains(PlayerInfo.EntityId));
	}
}
