using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Durango.System;
using Durango.UI;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public class UIBase : MonoBehaviour, IUriInvokable
{
	public delegate void PreCloseDelegate(ref bool res);

	public delegate void CurrencyChangedDelegate(IEnumerable<CurrencyData> from, IEnumerable<CurrencyData> to);

	public enum AnchorType
	{
		Default,
		Base,
		Fullscreen,
		Clone,
		CloneFullscreen,
		FullscreenMobileOnly
	}

	public enum UIType
	{
		Default = 0,
		Openable = 3
	}

	private static readonly List<UIBase> OpenableUIStack;

	private static UIBase _currentOpenedUI;

	private static int _screenOrientationLockCount;

	private static readonly List<UIBase> CloseWhenPlayerMoveUIList;

	public readonly Observable<bool> HasBack = new Observable<bool>();

	protected bool PlayCloseSound = true;

	protected UISound.GroupType _openCloseSound;

	[CanBeNull]
	[SerializeField]
	protected UIWidget _background;

	private TweenAlpha _openTweener;

	private UriMethods _uriMethods;

	private string _uriAttribute;

	[SerializeField]
	private bool _softOpen;

	[SerializeField]
	private bool _gameBlur;

	[SerializeField]
	private bool _closeWhenPlayerMoveStart;

	[SerializeField]
	private bool _lockScreenOrientation;

	[SerializeField]
	private UIType _uiType;

	[SerializeField]
	private AnchorType _anchorType;

	[SerializeField]
	private string _avatarMotion = string.Empty;

	[SerializeField]
	private List<CurrencyData> _currencyList;

	private float? _backgroundAlpha;

	protected virtual bool IsSoundOcclusion => Anchor != AnchorType.FullscreenMobileOnly;

	public UIWidget BackgroundWidget => _background;

	public static UIBase CurrentUI
	{
		get
		{
			return _currentOpenedUI;
		}
		private set
		{
			bool flag = true;
			bool flag2 = true;
			if (value == null)
			{
				if (_currentOpenedUI != null)
				{
					flag2 = _currentOpenedUI.Anchor != AnchorType.FullscreenMobileOnly;
					_currentOpenedUI.ForceClose();
				}
				if (OpenableUIStack.Count == 0)
				{
					_currentOpenedUI = null;
				}
				else
				{
					int index = OpenableUIStack.Count - 1;
					_currentOpenedUI = OpenableUIStack[index];
					OpenableUIStack.RemoveAt(index);
					_currentOpenedUI.SetVisible(visible: true, "UIStack");
					flag = _currentOpenedUI.Anchor != AnchorType.FullscreenMobileOnly;
				}
				if (UIBase.UIClosed != null)
				{
					UIBase.UIClosed();
				}
			}
			else
			{
				if (_currentOpenedUI != null)
				{
					flag2 = _currentOpenedUI.Anchor != AnchorType.FullscreenMobileOnly;
					_currentOpenedUI.SetVisible(visible: false, "UIStack");
					OpenableUIStack.Add(_currentOpenedUI);
				}
				flag = value.Anchor != AnchorType.FullscreenMobileOnly;
				_currentOpenedUI = value;
				if (UIBase.UIOpened != null)
				{
					UIBase.UIOpened();
				}
			}
			if (flag2 != flag && UIBase.FullscreenAnchorTypeChanged != null)
			{
				UIBase.FullscreenAnchorTypeChanged(flag);
			}
			if (flag2 != flag)
			{
				VisibleController.Hide((!flag2) ? new Predicate<VisibleController>(HideOnRightSide) : new Predicate<VisibleController>(HideOnFullscreen), hide: false, GetVisibleKey(flag2), 0.3f);
			}
			bool flag3 = _currentOpenedUI != null;
			VisibleController.Hide((!flag) ? new Predicate<VisibleController>(HideOnRightSide) : new Predicate<VisibleController>(HideOnFullscreen), flag3, GetVisibleKey(flag), (!flag3) ? 0.3f : 0f);
			SoundManager.SetState(new SoundStates("full_screen", (!(_currentOpenedUI != null) || !_currentOpenedUI.IsSoundOcclusion) ? "off" : "on"));
		}
	}

	public static UIBase PreviousUI
	{
		get
		{
			if (OpenableUIStack.Count > 0)
			{
				return OpenableUIStack[OpenableUIStack.Count - 1];
			}
			return null;
		}
	}

	public static int OpenedUICount => OpenableUIStack.Count;

	public static bool HasOpenedUI => CurrentUI != null;

	public static bool HasOpenedFullscreenUI
	{
		get
		{
			if (CurrentUI != null)
			{
				return CurrentUI.Anchor != AnchorType.FullscreenMobileOnly;
			}
			return false;
		}
	}

	public static bool IsLockScreenOrientation => _screenOrientationLockCount > 0;

	private TweenAlpha OpenTweener
	{
		get
		{
			if (_openTweener == null)
			{
				_openTweener = GetComponent<TweenAlpha>();
				if (_openTweener == null)
				{
					_openTweener = base.gameObject.AddComponent<TweenAlpha>();
				}
				_openTweener.from = 0f;
				_openTweener.to = 1f;
				_openTweener.duration = 0.3f;
				_openTweener.ResetToBeginning();
				_openTweener.enabled = false;
			}
			return _openTweener;
		}
	}

	public UIRect Rect { get; private set; }

	public VisibleController VisibleController { get; private set; }

	public bool Visible => VisibleController.Visible;

	public bool IsOpened { get; private set; }

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

	public AnchorType Anchor
	{
		get
		{
			if (Platform.Instance.UsePCUI)
			{
				if (_anchorType == AnchorType.CloneFullscreen)
				{
					return AnchorType.Fullscreen;
				}
			}
			else if (_anchorType == AnchorType.FullscreenMobileOnly)
			{
				return AnchorType.Fullscreen;
			}
			return _anchorType;
		}
	}

	public bool IsPortrait
	{
		get
		{
			if (Platform.Instance.SupportPortrait)
			{
				return UIManager.IsPortraitScreen;
			}
			return Anchor == AnchorType.FullscreenMobileOnly;
		}
	}

	public bool IsOpenable => _uiType == UIType.Openable;

	public bool GameBlur => _gameBlur;

	public ReadOnlyCollection<CurrencyData> CurrencyList => _currencyList.AsReadOnly();

	public static event Action UIOpened;

	public static event Action UIClosed;

	public static event PreCloseDelegate OnPreCloseUI;

	public static event Action OnScreenOrientationLock;

	public static event Action<bool> FullscreenAnchorTypeChanged;

	public event Action OnOpenSucceed;

	public event Action OnCloseSucceed;

	public event CurrencyChangedDelegate CurrencyChanged;

	static UIBase()
	{
		OpenableUIStack = new List<UIBase>();
		CloseWhenPlayerMoveUIList = new List<UIBase>();
		GameManager.Reset += delegate
		{
			OpenableUIStack.Clear();
			CloseWhenPlayerMoveUIList.Clear();
			UIBase.UIOpened = null;
			UIBase.UIClosed = null;
			UIBase.OnPreCloseUI = null;
			UIBase.OnScreenOrientationLock = null;
			_currentOpenedUI = null;
			_screenOrientationLockCount = 0;
		};
	}

	int IUriInvokable.InvokeUri(string[] tokens, int index)
	{
		if (_uriMethods == null)
		{
			_uriMethods = new UriMethods(this);
		}
		return _uriMethods.InvokeUri(tokens, index);
	}

	IEnumerable<string> IUriInvokable.CollectUri()
	{
		if (_uriMethods == null)
		{
			_uriMethods = new UriMethods(this);
		}
		return _uriMethods.CollectUri();
	}

	private static bool HideOnFullscreen(VisibleController script)
	{
		if ((script.Flag & VisibleType.Base) != 0)
		{
			return (script.Flag & VisibleType.VisibleOnFullScreen) == 0;
		}
		return false;
	}

	private static bool HideOnRightSide(VisibleController script)
	{
		return (script.Flag & VisibleType.HideOnRightSide) != 0;
	}

	private static string GetVisibleKey(bool fullscreen)
	{
		if (fullscreen)
		{
			return "Openable";
		}
		return "OpenableRightSide";
	}

	public static bool CloseUI()
	{
		if (UIBase.OnPreCloseUI != null)
		{
			bool res = false;
			UIBase.OnPreCloseUI(ref res);
			if (res)
			{
				return true;
			}
		}
		if (CurrentUI == null)
		{
			return false;
		}
		CurrentUI.Close();
		return true;
	}

	public static void CloseAllUI()
	{
		int num = 0;
		while (CloseUI())
		{
			num++;
			if (num > 100)
			{
				break;
			}
			UIBase currentUI = CurrentUI;
			if (currentUI == null)
			{
				continue;
			}
			if (!CloseUI())
			{
				break;
			}
			num++;
			if (CurrentUI == currentUI)
			{
				currentUI.ForceClose();
				if (CurrentUI == currentUI)
				{
					break;
				}
			}
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

	private void CheckRectEnabled()
	{
		Rect.enabled = IsOpened || _uiType == UIType.Default;
	}

	public void SetVisible(bool visible, string key, float duration = 0f)
	{
		VisibleController.SetVisible(visible, key, duration);
	}

	public void Init(UIWidget rootAnchor)
	{
		Rect = GetComponent<UIRect>();
		VisibleController = base.gameObject.AddMissingComponent<VisibleController>();
		if (rootAnchor != null)
		{
			base.transform.localScale = rootAnchor.transform.localScale;
		}
		UIAnchorPolicy.Instance.SetAnchor(this, rootAnchor);
		UIPanel uIPanel = Rect as UIPanel;
		if (uIPanel != null)
		{
			if (rootAnchor == null)
			{
				uIPanel.clipping = UIDrawCall.Clipping.None;
			}
			else if (uIPanel.clipping == UIDrawCall.Clipping.None)
			{
				uIPanel.clipping = UIDrawCall.Clipping.ConstrainButDontClip;
			}
		}
		CheckRectEnabled();
		UIManager.ScreenResized += OnScreenResized;
		object[] customAttributes = GetType().GetCustomAttributes(typeof(UriAttribute), inherit: true);
		for (int i = 0; i < KUtility.GetSize(customAttributes); i++)
		{
			string key = ((UriAttribute)customAttributes[i]).Key;
			_uriAttribute = key;
		}
	}

	public virtual bool Open()
	{
		if (IsOpened)
		{
			if (_uiType == UIType.Openable && CurrentUI != this && OpenableUIStack.Remove(this))
			{
				SetVisible(visible: true, "UIStack");
				CurrentUI = this;
			}
			return false;
		}
		if (CurrentUI != null)
		{
			UIType uiType = _uiType;
			if (uiType != UIType.Openable && uiType != 0)
			{
				return false;
			}
		}
		if (!TryOpen())
		{
			return false;
		}
		bool flag = _softOpen;
		IsOpened = true;
		CheckRectEnabled();
		if (_background != null)
		{
			if (Platform.Instance.UsePCUI && _background.gameObject.layer != LayerHelper.UIOverLayer)
			{
				_background.alpha = 1f;
			}
			else
			{
				float? backgroundAlpha = _backgroundAlpha;
				if (!backgroundAlpha.HasValue)
				{
					_backgroundAlpha = _background.alpha;
				}
				_background.alpha = _backgroundAlpha.Value * ((!TimeGauge.IsSunUp) ? 0.5f : 1f);
			}
		}
		if (this.OnOpenSucceed != null)
		{
			this.OnOpenSucceed();
		}
		if (_uiType == UIType.Openable)
		{
			flag &= CurrentUI == null;
			CurrentUI = this;
		}
		if (_closeWhenPlayerMoveStart)
		{
			CloseWhenPlayerMoveUIList.Add(this);
		}
		UISound.PlayOpenGroup(_openCloseSound);
		PlayCloseSound = true;
		if (!string.IsNullOrEmpty(_avatarMotion) && PlayerController.MotionUpdater.IsState("Stand"))
		{
			PlayerController.MotionUpdater.Motion(_avatarMotion);
		}
		if (flag)
		{
			OpenTweener.tweenFactor = 0f;
			OpenTweener.PlayForward();
		}
		else
		{
			Rect.alpha = 1f;
		}
		if (_lockScreenOrientation)
		{
			bool isLockScreenOrientation = IsLockScreenOrientation;
			_screenOrientationLockCount++;
			if (isLockScreenOrientation != IsLockScreenOrientation && UIBase.OnScreenOrientationLock != null)
			{
				UIBase.OnScreenOrientationLock();
			}
		}
		return true;
	}

	public virtual bool Close()
	{
		if (!IsOpened)
		{
			return false;
		}
		if (!TryClose())
		{
			return false;
		}
		IsOpened = false;
		CheckRectEnabled();
		if (_uiType == UIType.Openable)
		{
			CurrentUI = null;
		}
		if (PlayCloseSound)
		{
			UISound.PlayCloseGroup(_openCloseSound);
		}
		PlayerController.MotionUpdater.RefreshMotion(_avatarMotion);
		if (_lockScreenOrientation)
		{
			bool isLockScreenOrientation = IsLockScreenOrientation;
			_screenOrientationLockCount--;
			if (isLockScreenOrientation != IsLockScreenOrientation && UIBase.OnScreenOrientationLock != null)
			{
				UIBase.OnScreenOrientationLock();
			}
		}
		if (this.OnCloseSucceed != null)
		{
			this.OnCloseSucceed();
		}
		return true;
	}

	public void ForceClose()
	{
		int num = 0;
		while (IsOpened)
		{
			Close();
			num++;
			if (num > 100)
			{
				break;
			}
		}
	}

	protected virtual bool TryOpen()
	{
		if (_uiType == UIType.Openable && !MenuHelper.IsEnabledMenu(this))
		{
			return false;
		}
		SetChildrenActive(activated: true);
		return true;
	}

	protected virtual bool TryClose()
	{
		SetChildrenActive(activated: false);
		return true;
	}

	protected void SetChildrenActive(bool activated)
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).gameObject.SetActive(activated);
		}
	}

	protected void SetCurrencyList(IEnumerable<CurrencyData> currencies)
	{
		if (_currencyList == null)
		{
			_currencyList = new List<CurrencyData>();
		}
		if (this.CurrencyChanged != null)
		{
			List<CurrencyData> from = new List<CurrencyData>(_currencyList);
			_currencyList.Clear();
			_currencyList.AddRange(currencies);
			this.CurrencyChanged(from, _currencyList);
		}
		else
		{
			_currencyList.Clear();
			_currencyList.AddRange(currencies);
		}
	}

	private void SetScreenBackground(bool isScreen)
	{
		if (!(_background == null))
		{
			if (isScreen)
			{
				_background.leftAnchor.SetScreen(0f, 0f);
				_background.bottomAnchor.SetScreen(0f, 0f);
				_background.rightAnchor.SetScreen(1f, 0f);
				_background.topAnchor.SetScreen(1f, 0f);
			}
			else
			{
				Transform target = base.transform;
				_background.leftAnchor.Set(target, 0f, 0f);
				_background.bottomAnchor.Set(target, 0f, 0f);
				_background.rightAnchor.Set(target, 1f, 0f);
				_background.topAnchor.Set(target, 1f, 0f);
			}
			_background.ResetAndUpdateAnchors();
		}
	}

	protected virtual void OnScreenResized()
	{
		UIAnchorPolicy.Instance.SetBackgroundAnchor(this);
	}

	[Uri]
	protected virtual void DefaultUri()
	{
		Open();
	}
}
