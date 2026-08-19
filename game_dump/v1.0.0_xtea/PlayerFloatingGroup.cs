using System.Collections.Generic;
using UnityEngine;

public class PlayerFloatingGroup : UIBase
{
	[SerializeField]
	private IndicatorControl _indicatorBase;

	[SerializeField]
	private float _indicatorTimeMargin;

	[SerializeField]
	private GameObject _floatingUIBase;

	[SerializeField]
	private Color _localPlayerNameColor;

	[SerializeField]
	private Color _localPlayerCombatModeNameColor;

	[SerializeField]
	private Color _playerNameColor;

	[SerializeField]
	private Color _enemyPlayerNameColor;

	[SerializeField]
	private Color _localPlayerClanColor;

	[SerializeField]
	private Color _playerClanColor;

	[SerializeField]
	private Color _localPlayerStatusColor;

	[SerializeField]
	private Color _playerStatusColor;

	[SerializeField]
	private DamageWidgetIndicatorControl _damageWidgetControl;

	private readonly List<PlayerFloatingUIControl> _floatingUIList = new List<PlayerFloatingUIControl>();

	private Stack<IndicatorControl> _indicatorPool;

	private Queue<KeyValuePair<GameObject, string>> _waitIndicators;

	private float _nextIndicatorTime;

	private bool _hideLocalPlayer;

	public DamageWidgetIndicatorControl DamageWidgetControl => _damageWidgetControl;

	private void Awake()
	{
		_indicatorPool = new Stack<IndicatorControl>();
		_waitIndicators = new Queue<KeyValuePair<GameObject, string>>();
		((Component)_indicatorBase).gameObject.SetActive(false);
		_damageWidgetControl.Init();
	}

	private void Start()
	{
		KSingleton<PlayerManager>.Instance().PlayerAppeared += OnAppearPlayer;
		KSingleton<PlayerManager>.Instance().PlayerDisappeared += OnDisappearPlayer;
		KSingleton<PlayerManager>.Instance().PlayerClanChanged += OnPlayerClanChange;
		KSingleton<PlayerManager>.Instance().PlayerTitleChanged += OnPlayerTitleChange;
		GameSystem<ClanSystem>.Instance().ClanChanged += delegate
		{
			RefreshStates();
		};
		GameSystem<ClanSystem>.Instance().EnemyClansDirtied += RefreshStates;
		Refresh();
	}

	private void LateUpdate()
	{
		for (int num = _floatingUIList.Count - 1; num >= 0; num--)
		{
			PlayerFloatingUIControl playerFloatingUIControl = _floatingUIList[num];
			if ((Object)(object)playerFloatingUIControl.Target != (Object)null)
			{
				playerFloatingUIControl.Process(_hideLocalPlayer);
			}
			else
			{
				Remove(playerFloatingUIControl);
			}
		}
		if (_waitIndicators.Count != 0 && _nextIndicatorTime < Time.time)
		{
			_nextIndicatorTime = Time.time + _indicatorTimeMargin;
			KeyValuePair<GameObject, string> keyValuePair = _waitIndicators.Dequeue();
			IndicatorControl indicatorControl = Indicator_Pop();
			indicatorControl.Target = keyValuePair.Key;
			indicatorControl.Text = keyValuePair.Value;
			indicatorControl.Begin();
		}
	}

	private void OnAppearPlayer(PlayerBehavior player)
	{
		MakePlayerFloatingWidget(player);
	}

	private void OnDisappearPlayer(PlayerBehavior player)
	{
		Remove(GetWidget(player));
	}

	private void OnPlayerClanChange(PlayerBehavior player)
	{
		SetClan(player, player.Clan.ClanName);
	}

	private void OnPlayerTitleChange(PlayerBehavior player)
	{
		SetTitle(player, player.Title._Title);
	}

	private void Refresh()
	{
		List<PlayerBehavior> players = KSingleton<PlayerManager>.Instance().Players;
		MakePlayerFloatingWidget(PlayerBehavior.LocalPlayer);
		int i = 0;
		for (int count = players.Count; i < count; i++)
		{
			MakePlayerFloatingWidget(players[i]);
		}
	}

	private void RefreshStates()
	{
		int i = 0;
		for (int count = _floatingUIList.Count; i < count; i++)
		{
			RefreshLabelColor(_floatingUIList[i]);
		}
	}

	private PlayerFloatingUIControl GetWidget(PlayerBehavior player, bool make = false)
	{
		PlayerFloatingUIControl playerFloatingUIControl = null;
		int count = _floatingUIList.Count;
		for (int i = 0; i < count; i++)
		{
			if ((Object)(object)_floatingUIList[i].Target == (Object)(object)player)
			{
				playerFloatingUIControl = _floatingUIList[i];
				break;
			}
		}
		if (make && (Object)(object)playerFloatingUIControl == (Object)null)
		{
			playerFloatingUIControl = ((Component)this).gameObject.AddChild(_floatingUIBase.gameObject).GetComponent<PlayerFloatingUIControl>();
			playerFloatingUIControl.Target = player;
			((Component)playerFloatingUIControl).gameObject.SetActive(false);
			_floatingUIList.Add(playerFloatingUIControl);
		}
		return playerFloatingUIControl;
	}

	public void HideLocalPlayer()
	{
		_hideLocalPlayer = true;
	}

	private void MakePlayerFloatingWidget(PlayerBehavior player)
	{
		if (!((Object)(object)player == (Object)null))
		{
			PlayerFloatingUIControl widget = GetWidget(player, make: true);
			widget.SetName(player.PlayerName);
			widget.SetTitle(player.Title._Title);
			widget.SetClan(player.Clan.ClanName);
			RefreshLabelColor(widget);
		}
	}

	private void SetTitle(PlayerBehavior player, string title)
	{
		PlayerFloatingUIControl widget = GetWidget(player);
		if (!((Object)(object)widget == (Object)null))
		{
			widget.SetTitle(title);
		}
	}

	private void SetClan(PlayerBehavior player, string clan)
	{
		PlayerFloatingUIControl widget = GetWidget(player);
		if (!((Object)(object)widget == (Object)null))
		{
			widget.SetClan(clan);
			RefreshLabelColor(widget);
		}
	}

	public void UpdateNameColor(PlayerBehavior player)
	{
		PlayerFloatingUIControl widget = GetWidget(player);
		RefreshLabelColor(widget);
	}

	private void RefreshLabelColor(PlayerFloatingUIControl info)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)info == (Object)null))
		{
			if (info.Target.IsLocalPlayer)
			{
				info.SetNameColor((!info.Target.IsCombatMode) ? _localPlayerNameColor : _localPlayerCombatModeNameColor);
				info.SetClanColor(_localPlayerClanColor);
				info.SetStatusColor(_localPlayerStatusColor);
			}
			else
			{
				info.SetNameColor((!GameSystem<ClanSystem>.Instance().IsEnemyClan(info.Target.ClanId)) ? _playerNameColor : _enemyPlayerNameColor);
				info.SetClanColor(_playerClanColor);
				info.SetStatusColor(_playerStatusColor);
			}
		}
	}

	private void Remove(PlayerFloatingUIControl info)
	{
		if (!((Object)(object)info == (Object)null))
		{
			_floatingUIList.Remove(info);
			Object.Destroy((Object)(object)((Component)info).gameObject);
		}
	}

	private void Indicator_Push(IndicatorControl indicator)
	{
		((Component)indicator).gameObject.SetActive(false);
		_indicatorPool.Push(indicator);
	}

	private IndicatorControl Indicator_Pop()
	{
		IndicatorControl indicatorControl;
		if (_indicatorPool.Count == 0)
		{
			indicatorControl = ((Component)((Component)_indicatorBase).transform.parent).gameObject.AddChild(((Component)_indicatorBase).gameObject).GetComponent<IndicatorControl>();
			indicatorControl.OnBegin = Indicator_OnBegin;
			indicatorControl.OnEnd = Indicator_OnEnd;
		}
		else
		{
			indicatorControl = _indicatorPool.Pop();
		}
		((Component)indicatorControl).GetComponent<UIWidget>().alpha = 0f;
		((Component)indicatorControl).gameObject.SetActive(true);
		return indicatorControl;
	}

	private void Indicator_OnBegin(IndicatorControl indicator)
	{
	}

	private void Indicator_OnEnd(IndicatorControl indicator)
	{
		Indicator_Push(indicator);
	}

	[ExposedInEditor(null)]
	public void AddIndicator(string text, GameObject target = null)
	{
		_waitIndicators.Enqueue(new KeyValuePair<GameObject, string>(target, text));
	}
}
