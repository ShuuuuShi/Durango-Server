using System;
using System.Collections;
using System.Collections.Generic;
using AndroidKeyboard;
using ChatData;
using Crafting;
using ItemSystem;
using K1Network;
using L10N;
using Messages;
using MsgPack;
using Shared.Battle;
using Shared.Chat;
using UnityEngine;

public class UIManager : KSingleton<UIManager>
{
	public const string CraftSucceedSound = "Sound/Effect/UI/UI_Crafting_Success_01.wav";

	public const string LevelUpEffect = "Particle/FX_SkillUp_01.prefab";

	public const string TwinkleParticle = "Particle/FX_Prop_Mineral_Twinkle_01.prefab";

	public const string FoundSound = "Sound/Effect/Prop/Prop_Found_Mineral_01.wav";

	public const string WarpholeFoundEffect = "Particle/FX_Found_Warphole_01.prefab";

	private static int _uiLayer = -1;

	private static int _uiOverLayer = -1;

	private static bool _isKeyboardVisible;

	private UIRoot _uiRoot;

	private JoystickGroup _virtualStick;

	private LeftMenuListGroup _menuList;

	private PlayGuideHelperGroup _playGuideHelper;

	private SideEffectGroup _sideEffect;

	private PlayerFloatingGroup _playerFloatingGroup;

	private PopupGroup _popup;

	private MessageBox _messageBox;

	[SerializeField]
	public UIAtlas UIAtlas;

	[SerializeField]
	public UIAtlas IconAtlas;

	[SerializeField]
	public UIAtlas RGBAtlas;

	[SerializeField]
	public UIAtlas AdditiveAtlas;

	private UIAtlas[] _atlases;

	[SerializeField]
	public UIFont Font;

	private readonly List<KeyValuePair<UIBase.AnchorType, UIWidget>> _rootAnchors = new List<KeyValuePair<UIBase.AnchorType, UIWidget>>();

	public static Color UIYellow => PresetColor.UIYellow;

	public static Color UIGreen => PresetColor.UIGreen;

	public static Color UIRed => PresetColor.UIRed;

	public static Color UIGray => PresetColor.UIGray;

	public static Color UILightGray => PresetColor.UILightGray;

	public static Color UIMoreLightGray => PresetColor.UIMoreLightGray;

	public static Color UIWhite => PresetColor.UIWhite;

	public static Color UIBlack => PresetColor.UIBlack;

	public static Color UIDarkOrange => PresetColor.UIDarkOrange;

	public static Color UIMoreLightBrown => PresetColor.UIMoreLightBrown;

	public static int UILayer => (_uiLayer != -1) ? _uiLayer : (_uiLayer = LayerMask.NameToLayer("NGUI"));

	public static int UIOverLayer => (_uiOverLayer != -1) ? _uiOverLayer : (_uiOverLayer = LayerMask.NameToLayer("NGUI Over"));

	public static bool IsPortraitMode { get; private set; }

	public static bool IsLoadingCurtain { get; private set; }

	public static int ScreenWidth => KSingleton<UIManager>.Instance().UIRoot.manualWidth;

	public static int ScreenHeight => KSingleton<UIManager>.Instance().UIRoot.activeHeight;

	public UIRoot UIRoot
	{
		get
		{
			if ((Object)(object)_uiRoot == (Object)null)
			{
				GameObject val = GameObject.Find("UI Root");
				if ((Object)(object)val != (Object)null)
				{
					_uiRoot = val.GetComponent<UIRoot>();
				}
			}
			return _uiRoot;
		}
	}

	public JoystickGroup VirtualStick
	{
		get
		{
			if ((Object)(object)_virtualStick == (Object)null)
			{
				_virtualStick = UIManager.FindScript<JoystickGroup>();
			}
			return _virtualStick;
		}
	}

	public LeftMenuListGroup MenuList
	{
		get
		{
			if ((Object)(object)_menuList == (Object)null)
			{
				_menuList = UIManager.FindScript<LeftMenuListGroup>();
			}
			return _menuList;
		}
	}

	public PlayGuideHelperGroup PlayGuideHelper
	{
		get
		{
			if ((Object)(object)_playGuideHelper == (Object)null)
			{
				_playGuideHelper = UIManager.FindScript<PlayGuideHelperGroup>();
			}
			return _playGuideHelper;
		}
	}

	public SideEffectGroup SideEffect
	{
		get
		{
			if ((Object)(object)_sideEffect == (Object)null)
			{
				_sideEffect = UIManager.FindScript<SideEffectGroup>();
			}
			return _sideEffect;
		}
	}

	public PlayerFloatingGroup PlayerFloatingGroup
	{
		get
		{
			if ((Object)(object)_playerFloatingGroup == (Object)null)
			{
				_playerFloatingGroup = UIManager.FindScript<PlayerFloatingGroup>();
			}
			return _playerFloatingGroup;
		}
	}

	public static MapContext MapContext => (!KSingleton<UIManager>.HasInstance()) ? null : KSingleton<MapContext>.Instance();

	public static PopupGroup Popup
	{
		get
		{
			if (!KSingleton<UIManager>.HasInstance())
			{
				return null;
			}
			if ((Object)(object)KSingleton<UIManager>.Instance()._popup == (Object)null)
			{
				KSingleton<UIManager>.Instance()._popup = UIManager.FindScript<PopupGroup>();
			}
			return KSingleton<UIManager>.Instance()._popup;
		}
	}

	public static MessageBox MessageBox
	{
		get
		{
			if (!KSingleton<UIManager>.HasInstance())
			{
				return null;
			}
			if ((Object)(object)KSingleton<UIManager>.Instance()._messageBox == (Object)null)
			{
				KSingleton<UIManager>.Instance()._messageBox = UIManager.FindScript<MessageBox>();
			}
			return KSingleton<UIManager>.Instance()._messageBox;
		}
	}

	public UIAtlas[] Atlases
	{
		get
		{
			if (_atlases == null)
			{
				_atlases = new UIAtlas[4] { UIAtlas, IconAtlas, RGBAtlas, AdditiveAtlas };
			}
			return _atlases;
		}
	}

	public static event Action<int> KeyboardHeightUpdated;

	public static event Action PortraitModeChanged;

	public static bool IsUILayer(int layer)
	{
		return layer == UILayer || layer == UIOverLayer;
	}

	protected override void OnAwake()
	{
		InitUIGroups();
		SetPortraitMode(ScreenWidth < ScreenHeight);
		KSingleton<PlayerController>.Instance().MoveStarted += UIBase.OnPlayerMoveStart;
		Connections.Frontend.On(delegate(Announce msg, PacketHeader header)
		{
			if (KSingleton<UIManager>.HasInstance())
			{
				ChatStruct chatStruct = default(ChatStruct);
				chatStruct.EntityId = 1000uL;
				chatStruct.Name = T._("[ffbf00]시스템[-]");
				chatStruct.Body = new RadioNotice
				{
					Text = msg.Text
				};
				chatStruct.Type = ChannelType.System;
				ChatStruct chat = chatStruct;
				GameSystem<SocialSystem>.Instance().AddChat(chat);
				SystemMsg(msg.Text, 4f);
			}
		});
		Connections.Frontend.On(delegate(Response msg, PacketHeader header)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (msg.Success)
			{
				MessagePackObject val = default(MessagePackObject);
				if (Debug.isDebugBuild && msg.Data.TryGetValue(MessagePackObject.op_Implicit("info"), ref val))
				{
					string comment = ((MessagePackObject)(ref val)).AsString();
					SystemMsg(comment, 4f);
				}
			}
			else
			{
				DefaultFailResponseHandle(msg.Data);
			}
		});
		Connections.Frontend.On(delegate(Text msg, PacketHeader header)
		{
			SystemMsg(msg._Text, 4f);
		});
		Connections.Frontend.On<NotificationAdded>(OnNewsAlarm);
		Connections.Frontend.On<NotificationCanceled>(OnCancelNewsAlarm);
		IsLoadingCurtain = true;
		ScreenOrientationController.SetPortraitLock(ScreenOrientationController.PortraitLock.Loading);
		OnLoadingCurtainHidden(delegate
		{
			IsLoadingCurtain = false;
			ScreenOrientationController.SetPortraitUnlock(ScreenOrientationController.PortraitLock.Loading);
		});
	}

	private void Start()
	{
		CacheSounds();
		CacheEffects();
		GameSystem<ItemCraftingSystem>.Instance().CraftingFinished += OnCraftFinished;
		GameSystem<ItemCraftingSystem>.Instance().CraftFailed += OnCraftFailed;
		GameSystem<StatisticsSystem>.Instance().ExpGained += OnChangeExp;
		GameSystem<StatisticsSystem>.Instance().LevelChanged += OnChangeLevel;
		GameSystem<MapSystem>.Instance().OnExploreWarphole += OnExploreWarphole;
		GameSystem<MapSystem>.Instance().OnExploreCrater += OnExploreCrater;
		GameSystem<MapSystem>.Instance().OnExploreCrack += OnExploreCrack;
		GameSystem<MapSystem>.Instance().OnExplorePort += OnExplorePort;
		GameSystem<BuildSystem>.Instance().SetHomeSucceed += OnSetHomeSucceed;
		GameSystem<BuildSystem>.Instance().SetBaseSucceed += OnSetBaseSucceed;
		KSingleton<TerrainA6>.Instance().RegionPhaseChanged += RegionPhaseChanged;
		if (KSingleton<ScreenOrientationController>.Exist())
		{
			KSingleton<ScreenOrientationController>.Instance().PortraitModeChanged += OnChangeScreenOrientation;
			KSingleton<ScreenOrientationController>.Instance().ReadyToChange += OnReadyToChangeOrientation;
		}
		UIBase.OnOpenCloseableUI += OnOpenCloseableUI;
		UIBase.OnCloseCloseableUI += OnCloseCloseableUI;
		UIBase.OnPreCloseUI += OnPreCloseUI;
	}

	private void OnDisable()
	{
		UIBase.ClearAllStaticData();
	}

	private void Update()
	{
		int num = ((TouchScreenKeyboard.instance != null) ? TouchScreenKeyboard.instance.Height : 0);
		bool flag = num > 0;
		if (flag != _isKeyboardVisible)
		{
			_isKeyboardVisible = flag;
			int num2 = 0;
			num2 = (int)((float)ScreenHeight * ((float)num / (float)((!IsPortraitMode) ? DeviceInfo.FullScreenSize.y : DeviceInfo.FullScreenSize.x)));
			num2 += 100;
			if (UIManager.KeyboardHeightUpdated != null)
			{
				UIManager.KeyboardHeightUpdated(num2);
			}
		}
	}

	private void CacheSounds()
	{
		SoundManager.Cache("Sound/Effect/UI/UI_Crafting_Success_01.wav");
		SoundManager.Cache("Sound/Effect/Prop/Prop_Found_Mineral_01.wav");
	}

	private void CacheEffects()
	{
		ParticleManager.Cache("Particle/FX_SkillUp_01.prefab");
		ParticleManager.Cache("Particle/FX_Prop_Mineral_Twinkle_01.prefab");
		ParticleManager.Cache("Particle/FX_Found_Warphole_01.prefab");
	}

	public static void DefaultFailResponseHandle(MessagePackObjectDictionary data, object param = null)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (data == null)
		{
			return;
		}
		string text = string.Empty;
		MessagePackObject val = default(MessagePackObject);
		if (data.TryGetValue(MessagePackObject.op_Implicit("msg"), ref val))
		{
			text = ((MessagePackObject)(ref val)).AsString();
			SystemMsg(text, 4f);
		}
		if (data.TryGetValue(MessagePackObject.op_Implicit("error"), ref val))
		{
			string text2 = ((MessagePackObject)(ref val)).AsString();
			if (string.IsNullOrEmpty(text2))
			{
				text2 = text;
			}
			Debug.LogError((object)("(Remote) " + text2));
		}
		else
		{
			Debug.LogError((object)"(Remote) received undefined error");
		}
	}

	public void SetRootAnchor(UIBase.AnchorType type, int left, int bottom, int right, int top)
	{
		Transform transform = ((Component)UIRoot).transform;
		ChangeRootAnchor(type, left, bottom, right, top);
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			UIBase component = ((Component)transform.GetChild(i)).GetComponent<UIBase>();
			if (!((Object)(object)component == (Object)null) && component.Anchor == type)
			{
				UIUtility.UpdateAnchors(((Component)component).transform);
			}
		}
	}

	private void ChangeRootAnchor(UIBase.AnchorType type, int left, int bottom, int right, int top)
	{
		UIWidget rootAnchor = GetRootAnchor(type);
		if (!((Object)(object)rootAnchor == (Object)null))
		{
			rootAnchor.SetAnchor(((Component)UIRoot).gameObject, left, bottom, right, top);
		}
	}

	private void InitUIGroups()
	{
		UIBase.ClearAllStaticData();
		UIRoot uIRoot = UIRoot;
		if (!((Object)(object)uIRoot == (Object)null))
		{
			PrefabLinker component = ((Component)uIRoot).GetComponent<PrefabLinker>();
			if (!((Object)(object)component == (Object)null))
			{
				component.Load(UIInitFunc, UIFiliterFunc);
			}
		}
	}

	private static bool UIFiliterFunc(GameObject obj)
	{
		if (!Debug.isDebugBuild)
		{
			string name = ((Object)obj).name;
			if (name.Contains("Development") || name.Contains("CommandButton"))
			{
				return false;
			}
		}
		return true;
	}

	private static void UIInitFunc(GameObject obj)
	{
		UIBase component = obj.GetComponent<UIBase>();
		if ((Object)(object)component != (Object)null)
		{
			component.Init(GetRootAnchor(component.Anchor));
		}
	}

	private void RefreshAllUIAnchors()
	{
		Array values = Enum.GetValues(typeof(UIBase.AnchorType));
		if ((Object)(object)UIRoot == (Object)null)
		{
			return;
		}
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			UIWidget rootAnchor = GetRootAnchor((UIBase.AnchorType)(int)values.GetValue(i));
			if ((Object)(object)rootAnchor != (Object)null)
			{
				rootAnchor.ResetAndUpdateAnchors();
			}
		}
		Transform transform = ((Component)UIRoot).transform;
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(transform);
		while (stack.Count > 0)
		{
			Transform val = stack.Pop();
			UIRect component = ((Component)val).GetComponent<UIRect>();
			if ((Object)(object)component != (Object)null)
			{
				component.ResetAndUpdateAnchors();
				WidgetLayoutController component2 = ((Component)component).GetComponent<WidgetLayoutController>();
				if ((Object)(object)component2 != (Object)null && component2.IsRoot)
				{
					component2.UpdateLayout();
				}
			}
			else
			{
				UIAnchor component3 = ((Component)val).GetComponent<UIAnchor>();
				if ((Object)(object)component3 != (Object)null)
				{
					((Behaviour)component3).enabled = true;
				}
			}
			int j = 0;
			for (int childCount = val.childCount; j < childCount; j++)
			{
				stack.Push(val.GetChild(j));
			}
		}
		UIUtility.ResetAnUpdateAnchors(((Component)UIRoot).transform);
	}

	public void ResetUIAlpha()
	{
		TweenAlpha.Begin(((Component)UIRoot).gameObject, 1f, 1f);
	}

	public void StartHitEffect()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		SideEffect.StartSideEffect(Color.red);
	}

	public Transform FindTransform(string fullPathName)
	{
		Transform val = ((Component)UIRoot).transform.Find(fullPathName);
		if ((Object)(object)val == (Object)null)
		{
		}
		return val;
	}

	public void HighlightSprite(string fullPathName, bool active)
	{
		Transform val = FindTransform(fullPathName);
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		if (active)
		{
			TweenScale tweenScale = ((Component)val).gameObject.GetComponent<TweenScale>();
			if ((Object)(object)tweenScale == (Object)null)
			{
				tweenScale = ((Component)val).gameObject.AddComponent<TweenScale>();
			}
			tweenScale.style = UITweener.Style.PingPong;
			tweenScale.to.x = 1.1f;
			tweenScale.to.y = 1.1f;
			tweenScale.duration = 0.5f;
			TweenAlpha tweenAlpha = ((Component)val).gameObject.GetComponent<TweenAlpha>();
			if ((Object)(object)tweenAlpha == (Object)null)
			{
				tweenAlpha = ((Component)val).gameObject.AddComponent<TweenAlpha>();
			}
			tweenAlpha.style = UITweener.Style.PingPong;
			tweenAlpha.to = 0.5f;
			tweenAlpha.duration = 0.5f;
		}
		else
		{
			Object.Destroy((Object)(object)((Component)val).GetComponent<TweenScale>());
			Object.Destroy((Object)(object)((Component)val).GetComponent<TweenAlpha>());
		}
	}

	public void SetSleepMode(bool isSleep)
	{
		float alpha = ((!isSleep) ? 1 : 0);
		TweenAlpha.Begin(((Component)UIRoot).gameObject, 1f, alpha);
	}

	public void SetPortraitMode(bool isPortrait)
	{
		if (IsPortraitMode != isPortrait)
		{
			((Component)UIRoot).GetComponent<UIPanel>().alpha = 0f;
		}
		IsPortraitMode = isPortrait;
		((MonoBehaviour)this).StartCoroutine(CoSetPortraitMode());
		if (UIManager.PortraitModeChanged != null)
		{
			UIManager.PortraitModeChanged();
		}
	}

	private IEnumerator CoSetPortraitMode()
	{
		while (true)
		{
			int w = Screen.width;
			int h = Screen.height;
			if (IsPortraitMode)
			{
				if (w < h)
				{
					UIRoot.manualWidth = 720;
					break;
				}
			}
			else if (w > h)
			{
				UIRoot.manualWidth = 1280;
				break;
			}
			yield return null;
		}
		yield return null;
		TweenAlpha.Begin(((Component)UIRoot).gameObject, 0.3f, 1f);
		OnPortraitModeChange();
	}

	private void OnPortraitModeChange()
	{
		if (IsPortraitMode)
		{
			int screenWidth = ScreenWidth;
			int screenHeight = ScreenHeight;
			CloneGroup cloneGroup = UIManager.FindScript<CloneGroup>();
			int num = ((!((Object)(object)cloneGroup == (Object)null)) ? cloneGroup.BetweenMargin : 0);
			int num2 = ((!((Object)(object)cloneGroup == (Object)null)) ? cloneGroup.BottomMargin : 0);
			float num3 = (float)screenWidth / 1280f;
			int num4 = (int)((float)(screenHeight - (num2 + num)) * num3 / (1f + num3));
			ChangeRootAnchor(UIBase.AnchorType.Fullscreen, 0, num4 + num2 + num, 1280 - screenWidth, 0);
			ChangeRootAnchor(UIBase.AnchorType.Clone, 0, num2, 0, -screenHeight + num4 + num2);
		}
		else
		{
			ChangeRootAnchor(UIBase.AnchorType.Fullscreen, 0, 0, 0, 0);
			ChangeRootAnchor(UIBase.AnchorType.Clone, 0, 0, 0, 0);
		}
		RefreshAllUIAnchors();
		NGUITools.Broadcast("OnPortraitMode", IsPortraitMode);
	}

	private void OnChangeScreenOrientation(bool isPortriat)
	{
		SetPortraitMode(isPortriat);
	}

	private void OnReadyToChangeOrientation(bool isPortrait)
	{
		Popup.Tooltip<ScreenRotateButton>().Show(5f);
	}

	private void OnOpenCloseableUI()
	{
		ScreenOrientationController.SetPortraitLock(ScreenOrientationController.PortraitLock.UI);
		OnChangeCloseableUI();
	}

	private void OnCloseCloseableUI()
	{
		if (!UIBase.IsOpenUI)
		{
			ScreenOrientationController.SetPortraitUnlock(ScreenOrientationController.PortraitLock.UI);
			InteractionButtonGroup.RefreshInteractions();
		}
		OnChangeCloseableUI();
	}

	private void OnPreCloseUI(ref bool res)
	{
		MessageBox messageBox = MessageBox;
		if ((Object)(object)messageBox != (Object)null && messageBox.IsShow)
		{
			messageBox.Hide();
			res = true;
		}
		else
		{
			if (UIBase.IsOpenUI)
			{
				return;
			}
			if ((Object)(object)messageBox != (Object)null)
			{
				MessageBox.Show(T._("종료하시겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						Application.Quit();
					}
				});
			}
			else
			{
				Application.Quit();
			}
		}
	}

	private void OnChangeCloseableUI()
	{
		bool flag = false;
		if ((Object)(object)UIBase.FullScreenUI == (Object)null)
		{
			for (int i = 0; i < UIBase.CloseableUIList.Count; i++)
			{
				if (UIBase.CloseableUIList[i].GameBlur)
				{
					flag = true;
				}
			}
		}
		else
		{
			flag = UIBase.FullScreenUI.GameBlur;
		}
		if (flag)
		{
			BlurController.BlurOn("Fullscreen", BlurController.Mask.Game);
		}
		else
		{
			BlurController.BlurOff("Fullscreen");
		}
	}

	public void ToggleClickEventHandler(string fullPathName, UIEventListener.VoidDelegate handler, bool add)
	{
		Transform val = FindTransform(fullPathName);
		if (!((Object)(object)val == (Object)null))
		{
			UIEventListener uIEventListener = UIEventListener.Get(((Component)val).gameObject);
			if (add)
			{
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, handler);
			}
			else
			{
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onClick, handler);
			}
		}
	}

	public Vector3 GetTargetNGUIRootPos(string fullPathName)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Transform val = FindTransform(fullPathName);
		if ((Object)(object)val == (Object)null)
		{
			return Vector3.zero;
		}
		return MainCamera.NGUILocalPositionToNGUIPosition(val.localPosition, val.parent);
	}

	public void Emoticon(uint type, float power)
	{
		KSingleton<EmoticonEffectControl>.Instance().Show(GameManager.PlayerId, type, power, findLocalPlayer: true);
		PlayerEmoticon msg = default(PlayerEmoticon);
		msg.EmoticonType = type;
		msg.Power = power;
		KSingleton<PlayerController>.Instance().FillPlayerInfo(out msg.PlayerInfo);
		msg.SentAt = Connections.Frontend.GetPredictedServerTime();
		Connections.Frontend.Send(msg);
		GameSystem<SocialSystem>.Instance().Say($"[FFFFFF][emoticon_{type + 1}][-]");
		if (!GameSystem<PlayerStatusEffectSystem>.Instance().IsActivated("do_not_encourage") && type == 0)
		{
			Connections.Frontend.Send(default(Encourage));
		}
	}

	private void OnCraftFinished(IList<ItemData> items, string recipe)
	{
		SoundManager.Play("Sound/Effect/UI/UI_Crafting_Success_01.wav");
		int i = 0;
		for (int size = KUtility.GetSize(items); i < size; i++)
		{
			ItemData itemData = items[i];
			string arg = T._("{1:lv:} [ffc000]{0}[-] 제작 성공", itemData.Name, itemData.Level);
			arg = string.Format("[{1}:1.5] {0}", arg, itemData.Icon);
			string comment = Util.ItemQualityString(itemData);
			RewardAlarmGroup rewardAlarmGroup = UIManager.FindScript<RewardAlarmGroup>();
			if ((Object)(object)rewardAlarmGroup != (Object)null)
			{
				rewardAlarmGroup.Show(arg, comment, RewardAlarmGroup.RewardEffectType.Craft);
			}
		}
	}

	private void OnCraftFailed(string recipeId, ActionInfo actionInfo)
	{
		Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(recipeId);
		string title = T._("[ffc000]{0}[-] 제작 실패", recipe.Name);
		string comment = Util.ActionInfoDetailString(actionInfo, craft: true);
		RewardAlarmGroup rewardAlarmGroup = UIManager.FindScript<RewardAlarmGroup>();
		if ((Object)(object)rewardAlarmGroup != (Object)null)
		{
			rewardAlarmGroup.Show(title, comment, RewardAlarmGroup.RewardEffectType.Craft);
		}
	}

	private void OnChangeExp(int exp, int bonusExp)
	{
		if (exp + bonusExp > 0)
		{
			IndicatorMsg((bonusExp <= 0) ? $"[icon=icon_exp:1.6] [3aa958]{exp}" : $"[icon=icon_exp_x2:2] [3aa958]{exp}");
		}
	}

	private void OnChangeLevel(int prev, int current)
	{
		if (prev != -1 && prev < current)
		{
			KSingleton<PlayerController>.Instance().ParticleEffect("Particle/FX_SkillUp_01.prefab");
		}
	}

	private void OnExploreWarphole(Point2 tile)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		DoFoundEvent(T._("워프홀을 발견했습니다"), "Particle/FX_Found_Warphole_01.prefab", TerrainA6.TilePositionToClientPosition(tile + Point2.one));
	}

	private void OnExploreCrater(Point2 tile)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		DoFoundEvent(T._("크레이터를 발견했습니다"), "Particle/FX_Found_Warphole_01.prefab", TerrainA6.TilePositionToClientPosition(tile + Point2.one));
	}

	private void OnExploreCrack(Point2 tile)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		DoFoundEvent(T._("크레이터를 발견했습니다"), "Particle/FX_Found_Warphole_01.prefab", TerrainA6.TilePositionToClientPosition(tile + Point2.one * 2));
	}

	private void OnExplorePort(Point2 tile)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		DoFoundEvent(T._("항구를 발견했습니다"), "Particle/FX_Prop_Mineral_Twinkle_01.prefab", TerrainA6.TilePositionToClientPosition(tile + Point2.one));
	}

	private void OnNewsAlarm(NotificationAdded msg, PacketHeader header)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double num = ((!msg.Since.HasValue) ? predictedServerTime : msg.Since.Value);
		double num2 = ((!msg.Until.HasValue) ? (predictedServerTime + 86400.0) : msg.Until.Value);
		float time = Time.time;
		float since = time + (float)(num - predictedServerTime);
		float until = time + (float)(num2 - predictedServerTime);
		Popup.NewsAlarm.Register(msg.Id, msg.Text, since, until, msg.Period);
	}

	private void OnCancelNewsAlarm(NotificationCanceled msg, PacketHeader header)
	{
		Popup.NewsAlarm.Remove(msg.Id);
	}

	private static void DoFoundEvent(string msg, string particle, Vector3 pos)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		SystemMsg(msg, 3f);
		ParticleManager.Emit(particle, pos, Quaternion.identity);
		SoundManager.Play("Sound/Effect/Prop/Prop_Found_Mineral_01.wav");
	}

	private void RegionPhaseChanged()
	{
		SystemMsg(T._("섬에 지각 변동이 일어나 새로운 자원이 생겨났습니다."), 5f);
		KSingleton<CameraShaker>.Instance().Shake(50f, 50f, 0.04f, 3f, 0.9f);
	}

	private void OnSetHomeSucceed()
	{
		SystemMsg(T._("이제 지도 보기에서 기반섬으로 귀환을 누르면 이곳으로 돌아옵니다."));
	}

	private void OnSetBaseSucceed()
	{
		SystemMsg(T._("이제 지도 보기에서 전초기지섬으로 귀환을 누르면 이곳으로 돌아옵니다."));
	}

	public static UIWidget GetRootAnchor(UIBase.AnchorType type)
	{
		List<KeyValuePair<UIBase.AnchorType, UIWidget>> rootAnchors = KSingleton<UIManager>.Instance()._rootAnchors;
		for (int i = 0; i < rootAnchors.Count; i++)
		{
			if (rootAnchors[i].Key == type)
			{
				return rootAnchors[i].Value;
			}
		}
		string text = null;
		if (type != 0)
		{
			text = $"Anchor.{type}";
		}
		UIWidget uIWidget = null;
		if (!string.IsNullOrEmpty(text))
		{
			UIRoot uIRoot = KSingleton<UIManager>.Instance().UIRoot;
			Transform val = ((Component)uIRoot).transform.FindChild(text);
			uIWidget = ((!((Object)(object)val == (Object)null)) ? ((Component)val).gameObject.GetComponent<UIWidget>() : null);
		}
		rootAnchors.Add(new KeyValuePair<UIBase.AnchorType, UIWidget>(type, uIWidget));
		return uIWidget;
	}

	public static void OnLoadingCurtainHidden(EventDelegate.Callback func)
	{
		LoadingCurtainGroup loadingCurtainGroup = UIManager.FindScript<LoadingCurtainGroup>();
		if ((Object)(object)loadingCurtainGroup != (Object)null && loadingCurtainGroup.IsVisible)
		{
			EventDelegate.Add(loadingCurtainGroup.FadeOutFinished, func, oneShot: true);
		}
		else
		{
			func();
		}
	}

	public static void OnHitLocalPlayer()
	{
		KSingleton<UIManager>.Instance().StartHitEffect();
	}

	public static void AddDamageLabel(CharacterBehavior character, Damage damage, CharacterBehavior attacker)
	{
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		PlayerBehavior component = ((Component)character).GetComponent<PlayerBehavior>();
		CharacterBehavior component2 = ((Component)character).GetComponent<CharacterBehavior>();
		bool flag = false;
		Color color;
		if (Object.op_Implicit((Object)(object)component))
		{
			DamageLabelIndicator damageLabelIndicator = KSingleton<DamageIndicator>.Instance().AddDamageIndicator(component, attacker, damage.Part);
			if (component.IsLocalPlayer)
			{
				flag = true;
			}
			if ((Object)(object)damageLabelIndicator == (Object)null)
			{
				return;
			}
			color = (Color)((!flag) ? new Color(1f, 0.73f, 0f) : Color.red);
			if (flag)
			{
				if (damage.Result == DamageResult.Missed)
				{
					((Color)(ref color))._002Ector(1f, 0.5f, 0.5f);
				}
				else if (damage.Value <= 0 || damage.Result == DamageResult.Guarded || damage.Result == DamageResult.AutoGuarded)
				{
					((Color)(ref color))._002Ector(0.35f, 0.8f, 1f);
				}
			}
			damageLabelIndicator.Begin(GetDamageString(damage, flag), color);
		}
		else
		{
			if (!Object.op_Implicit((Object)(object)component2))
			{
				return;
			}
			DamageWidgetIndicator damageWidgetIndicator = KSingleton<UIManager>.Instance().PlayerFloatingGroup.DamageWidgetControl.AddDamageIndicator(component2, attacker, damage.Part);
			if ((Object)(object)damageWidgetIndicator != (Object)null)
			{
				damageWidgetIndicator.SetData(damage);
				damageWidgetIndicator.Begin();
			}
			DamageLabelIndicator damageLabelIndicator2 = KSingleton<DamageIndicator>.Instance().AddDamageIndicator(component2, attacker, damage.Part);
			if (!((Object)(object)damageLabelIndicator2 == (Object)null))
			{
				color = Color.yellow;
				if (damage.Result == DamageResult.Missed)
				{
					((Color)(ref color))._002Ector(0.5f, 0.5f, 0.5f);
				}
				damageLabelIndicator2.Begin(GetDamageString(damage), color);
			}
		}
	}

	public static string GetDamageString(Damage damage, bool showCrossCounter = false)
	{
		if (showCrossCounter && (damage.Effects & DamageEffects.CrossCounter) > DamageEffects.None)
		{
			KSingleton<CameraShaker>.Instance().Shake(50f, 50f, 0.02f, 0.7f, 0.7f);
			return T._("크로스카운터");
		}
		switch (damage.Result)
		{
		case DamageResult.Dodged:
		case DamageResult.AutoDodged:
			return T._("피함");
		case DamageResult.Evaded:
			return T._("피함");
		case DamageResult.Guarded:
		case DamageResult.AutoGuarded:
			return damage.Value.ToString();
		case DamageResult.Missed:
			return damage.Value.ToString();
		case DamageResult.Hit:
			return damage.Value.ToString();
		default:
			return damage.Result.ToString();
		}
	}

	public static TV FindScript<TV>() where TV : Component
	{
		if (!KSingleton<UIManager>.Exist())
		{
			return (TV)(object)null;
		}
		UIRoot uIRoot = KSingleton<UIManager>.Instance().UIRoot;
		TV result = (TV)(object)null;
		PrefabLinker prefabLinker = ((!((Object)(object)uIRoot == (Object)null)) ? ((Component)uIRoot).GetComponent<PrefabLinker>() : null);
		if ((Object)(object)prefabLinker != (Object)null)
		{
			result = prefabLinker.FindScript<TV>();
		}
		return result;
	}

	public static TV Open<TV>() where TV : UIBase
	{
		TV val = UIManager.FindScript<TV>();
		if ((Object)(object)val == (Object)null)
		{
			return (TV)null;
		}
		val.Open();
		return val;
	}

	public static void ShowLoadingIcon(bool show)
	{
		if ((Object)(object)Popup != (Object)null)
		{
			Popup.IsLoading = show;
		}
	}

	public static void SystemMsg(string comment, float duration = 1f, Action onClick = null)
	{
		SystemMsg(null, comment, duration, onClick);
	}

	public static void SystemMsg(string key, string comment, float duration = 1f, Action onClick = null)
	{
		if (!string.IsNullOrEmpty(comment) && KSingleton<UIManager>.HasInstance())
		{
			SystemMsgGroup systemMsgGroup = UIManager.FindScript<SystemMsgGroup>();
			systemMsgGroup.PushMessage(key, comment, duration, onClick);
		}
	}

	public static void IndicatorMsg(string text, GameObject target = null)
	{
		KSingleton<UIManager>.Instance().PlayerFloatingGroup.AddIndicator(text, target);
	}

	public static void IgnoreUIDrag(GameObject go, Vector2 delta)
	{
		SetCurrentUITouchEvent(enable: false);
	}

	public static void SetCurrentUITouchEvent(bool enable)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!KSingleton<PlayerController>.HasInstance())
		{
			return;
		}
		int touchCount = Input.touchCount;
		PlayerController.TouchEvent touchEvent;
		for (int i = 0; i < touchCount; i++)
		{
			PlayerController playerController = KSingleton<PlayerController>.Instance();
			Touch touch = Input.GetTouch(i);
			touchEvent = playerController.FindTouch(((Touch)(ref touch)).fingerId);
			if (touchEvent != null)
			{
				touchEvent.IsNguiTouched = enable;
			}
		}
		touchEvent = KSingleton<PlayerController>.Instance().FindTouch(-10);
		if (touchEvent != null)
		{
			touchEvent.IsNguiTouched = enable;
		}
	}

	public static string ColorBBCode(Color c)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return $"[{NGUIText.EncodeColor(c)}]";
	}
}
