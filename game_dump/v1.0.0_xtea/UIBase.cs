using System;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
	public enum UIType
	{
		Default = 0,
		Closeable = 1,
		FullScreen = 3
	}

	public enum AnchorType
	{
		Default,
		Base,
		Fullscreen,
		Clone
	}

	[Flags]
	public enum UIFlag
	{
		Base = 1,
		CoveredByClosable = 2,
		HideToCombat = 4,
		CutScene = 8
	}

	public delegate void PreCloseDelegate(ref bool res);

	public static List<UIBase> List = new List<UIBase>();

	private static Stack<UIBase> _fullScreenUIStack;

	private static UIBase _fullScreenUI;

	private static List<UIBase> _closeableUIList;

	private static List<UIBase> _closeWhenPlayerMoveUIList;

	protected bool PlayCloseSound = true;

	private string _openSound;

	private string _closeSound;

	private TweenAlpha _openTweener;

	private UIRect _uiRect;

	private bool _visible = true;

	private readonly HashSet<string> _visibleKey = new HashSet<string>();

	private float _alpha = 1f;

	[SerializeField]
	private bool _softOpen;

	[SerializeField]
	private bool _gameBlur;

	[SerializeField]
	private bool _closeWhenPlayerMoveStart;

	[SerializeField]
	private UIType _uiType;

	[SerializeField]
	private UIFlag _uiFlag;

	[SerializeField]
	private AnchorType _anchorType;

	[SerializeField]
	private string _avatarMotion = string.Empty;

	private static Stack<UIBase> FullScreenUIStack
	{
		get
		{
			if (_fullScreenUIStack == null)
			{
				_fullScreenUIStack = new Stack<UIBase>();
			}
			return _fullScreenUIStack;
		}
	}

	public static UIBase FullScreenUI
	{
		get
		{
			return _fullScreenUI;
		}
		private set
		{
			if ((Object)(object)value == (Object)null)
			{
				if ((Object)(object)_fullScreenUI != (Object)null)
				{
					_fullScreenUI.ForceClose();
				}
				if (FullScreenUIStack.Count == 0)
				{
					_fullScreenUI = null;
				}
				else
				{
					_fullScreenUI = FullScreenUIStack.Pop();
					_fullScreenUI.SetVisible(visible: true, "UIStack");
				}
				if (UIBase.OnCloseCloseableUI != null)
				{
					UIBase.OnCloseCloseableUI();
				}
			}
			else
			{
				if ((Object)(object)_fullScreenUI != (Object)null)
				{
					_fullScreenUI.SetVisible(visible: false, "UIStack");
					_fullScreenUIStack.Push(_fullScreenUI);
				}
				_fullScreenUI = value;
				if (UIBase.OnOpenCloseableUI != null)
				{
					UIBase.OnOpenCloseableUI();
				}
			}
			if ((Object)(object)_fullScreenUI != (Object)null)
			{
				while (HasCloseable)
				{
					CloseUI(CloseableUIList.Count - 1, forceClose: true);
				}
			}
			bool hide = (Object)(object)_fullScreenUI != (Object)null;
			HideUI(UIFlag.Base, hide, "FullScreen");
		}
	}

	public static bool HasCloseable => CloseableUIList.Count > 0;

	public static bool IsOpenUI => HasCloseable || (Object)(object)FullScreenUI != (Object)null;

	public static List<UIBase> CloseableUIList
	{
		get
		{
			if (_closeableUIList == null)
			{
				_closeableUIList = new List<UIBase>();
			}
			return _closeableUIList;
		}
	}

	protected static List<UIBase> CloseWhenPlayerMoveUIList
	{
		get
		{
			if (_closeWhenPlayerMoveUIList == null)
			{
				_closeWhenPlayerMoveUIList = new List<UIBase>();
			}
			return _closeWhenPlayerMoveUIList;
		}
	}

	private TweenAlpha OpenTweener
	{
		get
		{
			if ((Object)(object)_openTweener == (Object)null)
			{
				_openTweener = ((Component)this).GetComponent<TweenAlpha>();
				if ((Object)(object)_openTweener == (Object)null)
				{
					_openTweener = ((Component)this).gameObject.AddComponent<TweenAlpha>();
				}
				_openTweener.from = 0f;
				_openTweener.to = 1f;
				_openTweener.duration = 0.3f;
				_openTweener.ResetToBeginning();
				((Behaviour)_openTweener).enabled = false;
			}
			return _openTweener;
		}
	}

	private UIRect UIRect
	{
		get
		{
			if ((Object)(object)_uiRect == (Object)null)
			{
				_uiRect = ((Component)this).GetComponent<UIRect>();
			}
			return _uiRect;
		}
	}

	public bool Visible => _visible;

	public float Alpha
	{
		get
		{
			return _alpha;
		}
		set
		{
			_alpha = value;
			UpdateAlpha();
		}
	}

	public bool IsOpen { get; set; }

	public bool SoftOpen
	{
		get
		{
			return _softOpen;
		}
		set
		{
			_softOpen = value;
		}
	}

	public AnchorType Anchor => _anchorType;

	public UIFlag Flag
	{
		get
		{
			return _uiFlag;
		}
		set
		{
			_uiFlag = value;
		}
	}

	public bool GameBlur => _gameBlur;

	public static event Action OnOpenCloseableUI;

	public static event Action OnCloseCloseableUI;

	public static event PreCloseDelegate OnPreCloseUI;

	public event Action OnOpenSucceed;

	public event Action OnCloseSucceed;

	public event Action<bool> OnVisible;

	private static void CloseUI(int index, bool forceClose)
	{
		if (index >= 0 && index < CloseableUIList.Count)
		{
			if (forceClose)
			{
				CloseableUIList[index].ForceClose();
			}
			else
			{
				CloseableUIList[index].Close();
			}
		}
	}

	public static void CloseUI(bool forceClose = false)
	{
		if (UIBase.OnPreCloseUI != null)
		{
			bool res = false;
			UIBase.OnPreCloseUI(ref res);
			if (res)
			{
				return;
			}
		}
		if ((Object)(object)FullScreenUI != (Object)null)
		{
			if (forceClose)
			{
				while ((Object)(object)FullScreenUI != (Object)null)
				{
					FullScreenUI.Close();
				}
			}
			else
			{
				FullScreenUI.Close();
			}
		}
		else
		{
			CloseUI(CloseableUIList.Count - 1, forceClose);
		}
	}

	public static void CloseAllUI()
	{
		while (HasCloseable || (Object)(object)FullScreenUI != (Object)null)
		{
			CloseUI(forceClose: true);
		}
	}

	private static void RemoveCloseable(UIBase ui)
	{
		CloseableUIList.Remove(ui);
		if (CloseableUIList.Count == 0)
		{
			HideUI(UIFlag.CoveredByClosable, hide: false, "CloseableUI");
		}
		if (UIBase.OnCloseCloseableUI != null)
		{
			UIBase.OnCloseCloseableUI();
		}
	}

	private static void AddCloseable(UIBase closeableUI)
	{
		CloseableUIList.Add(closeableUI);
		HideUI(UIFlag.CoveredByClosable, hide: true, "CloseableUI");
		if (UIBase.OnOpenCloseableUI != null)
		{
			UIBase.OnOpenCloseableUI();
		}
	}

	public static void OnPlayerMoveStart()
	{
		int i = 0;
		for (int count = CloseWhenPlayerMoveUIList.Count; i < count; i++)
		{
			CloseWhenPlayerMoveUIList[i].ForceClose();
		}
		CloseWhenPlayerMoveUIList.Clear();
	}

	public static void HideUI(UIFlag flag, bool hide, string key = null)
	{
		int i = 0;
		for (int count = List.Count; i < count; i++)
		{
			if ((List[i]._uiFlag & flag) != 0)
			{
				List[i].SetVisible(!hide, key);
			}
		}
	}

	public static void HideUIExceptFor(UIFlag flag, bool hide, string key = null)
	{
		int i = 0;
		for (int count = List.Count; i < count; i++)
		{
			if ((List[i]._uiFlag & flag) == 0)
			{
				List[i].SetVisible(!hide, key);
			}
		}
	}

	public static void ClearAllStaticData()
	{
		FullScreenUIStack.Clear();
		CloseableUIList.Clear();
		CloseWhenPlayerMoveUIList.Clear();
		List.Clear();
		UIBase.OnOpenCloseableUI = null;
		UIBase.OnCloseCloseableUI = null;
		UIBase.OnPreCloseUI = null;
		FullScreenUI = null;
	}

	public void SetVisible(bool visible, string key = null)
	{
		if (visible)
		{
			if (!string.IsNullOrEmpty(key))
			{
				_visibleKey.Remove(key);
			}
			if (_visibleKey.Count > 0)
			{
				return;
			}
		}
		else if (!string.IsNullOrEmpty(key))
		{
			_visibleKey.Add(key);
		}
		_visible = visible;
		TweenAlpha component = ((Component)this).GetComponent<TweenAlpha>();
		if (visible)
		{
			if ((Object)(object)component != (Object)null && ((Behaviour)component).enabled && component.to < 1f)
			{
				((Behaviour)component).enabled = false;
			}
		}
		else if ((Object)(object)component != (Object)null && ((Behaviour)component).enabled)
		{
			((Behaviour)component).enabled = false;
		}
		if ((Object)(object)component == (Object)null || !((Behaviour)component).enabled)
		{
			UpdateAlpha();
		}
		if (this.OnVisible != null)
		{
			this.OnVisible(_visible);
		}
	}

	private void UpdateAlpha()
	{
		UIRect.alpha = ((!Visible) ? 0f : _alpha);
	}

	public void Init(UIWidget rootAnchor)
	{
		if (!List.Contains(this))
		{
			List.Add(this);
		}
		UIRect uIRect = UIRect;
		uIRect.SetAnchor((!((Object)(object)rootAnchor == (Object)null)) ? ((Component)rootAnchor).gameObject : null, 0, 0, 0, 0);
		UIPanel uIPanel = uIRect as UIPanel;
		if ((Object)(object)uIPanel != (Object)null)
		{
			if ((Object)(object)rootAnchor == (Object)null)
			{
				uIPanel.clipping = UIDrawCall.Clipping.None;
			}
			else if (uIPanel.clipping == UIDrawCall.Clipping.None)
			{
				uIPanel.clipping = UIDrawCall.Clipping.ConstrainButDontClip;
			}
		}
	}

	public virtual void Open()
	{
		if (IsOpen)
		{
			return;
		}
		if ((Object)(object)FullScreenUI != (Object)null)
		{
			switch (_uiType)
			{
			default:
				return;
			case UIType.Closeable:
			case (UIType)2:
				return;
			case UIType.Default:
			case UIType.FullScreen:
				break;
			}
		}
		if (!OnOpen())
		{
			return;
		}
		IsOpen = true;
		if (this.OnOpenSucceed != null)
		{
			this.OnOpenSucceed();
		}
		switch (_uiType)
		{
		case UIType.Closeable:
			AddCloseable(this);
			break;
		case UIType.FullScreen:
			FullScreenUI = this;
			break;
		}
		if (_closeWhenPlayerMoveStart)
		{
			CloseWhenPlayerMoveUIList.Add(this);
		}
		SoundManager.Play(_openSound);
		PlayCloseSound = true;
		if (!string.IsNullOrEmpty(_avatarMotion))
		{
			PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
			if (!localPlayer.IsCombatMode && localPlayer.IsCurrentAnimState("Stand"))
			{
				KSingleton<PlayerController>.Instance().Motion(_avatarMotion);
			}
		}
		if (_softOpen)
		{
			OpenTweener.tweenFactor = 0f;
			OpenTweener.PlayForward();
		}
		else
		{
			UpdateAlpha();
		}
	}

	public virtual void Close()
	{
		if (IsOpen && OnClose())
		{
			IsOpen = false;
			switch (_uiType)
			{
			case UIType.Closeable:
				RemoveCloseable(this);
				break;
			case UIType.FullScreen:
				FullScreenUI = null;
				break;
			}
			if (PlayCloseSound)
			{
				SoundManager.Play(_closeSound);
			}
			PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
			if (!string.IsNullOrEmpty(_avatarMotion) && !localPlayer.IsCombatMode && localPlayer.CurrentAnimClipInfo != null && localPlayer.CurrentAnimClipInfo.Clip == _avatarMotion)
			{
				KSingleton<PlayerController>.Instance().RefreshMotion(string.Empty);
			}
			if (this.OnCloseSucceed != null)
			{
				this.OnCloseSucceed();
			}
		}
	}

	public void ForceClose()
	{
		int num = 0;
		while (IsOpen)
		{
			Close();
			num++;
			if (num > 100)
			{
				break;
			}
		}
	}

	protected virtual bool OnOpen()
	{
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			((Component)((Component)this).transform.GetChild(i)).gameObject.SetActive(true);
		}
		return true;
	}

	protected virtual bool OnClose()
	{
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			((Component)((Component)this).transform.GetChild(i)).gameObject.SetActive(false);
		}
		return true;
	}

	protected void SetOpenCloseSound(string open, string close)
	{
		_openSound = open;
		_closeSound = close;
		SoundManager.Cache(_openSound);
		SoundManager.Cache(_closeSound);
	}
}
