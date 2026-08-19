using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Durango.Logic.Social;
using Durango.Logic.Timeline;
using Durango.Network;
using Durango.Render.Camera;
using Durango.System;
using Durango.UI;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.UI.Prologue;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Chat;
using UnityEngine;

public class UIManager : Singleton<UIManager>, IUriInvokable
{
	[CompilerGenerated]
	private sealed class _003CCoUpdateScreenSize_003Ed__72 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public UIManager _003C_003E4__this;

		private UIPanel _003Cp_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoUpdateScreenSize_003Ed__72(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cp_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			UIManager uIManager = _003C_003E4__this;
			int width;
			int height;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cp_003E5__2 = uIManager.UIRoot.GetComponent<UIPanel>();
				if (uIManager._isInitScreenSize)
				{
					_003Cp_003E5__2.alpha = 0f;
				}
				else
				{
					_003Cp_003E5__2.alpha = 1f;
					uIManager._isInitScreenSize = true;
				}
				goto IL_0084;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0084;
			case 2:
				{
					_003C_003E1__state = -1;
					_003Cp_003E5__2.alpha = 1f;
					uIManager.OnScreenSizeChanged();
					return false;
				}
				IL_0084:
				if (!Platform.Instance.GetScreenResolution(IsPortraitScreen, out width, out height))
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				uIManager.UIRoot.manualWidth = width;
				uIManager.UIRoot.manualHeight = height;
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CCollectUri_003Ed__57 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public UIManager _003C_003E4__this;

		private IEnumerator<KeyValuePair<string, IUriInvokable>> _003C_003E7__wrap1;

		private KeyValuePair<string, IUriInvokable> _003Cinvoker_003E5__3;

		private IEnumerator<string> _003C_003E7__wrap3;

		string IEnumerator<string>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCollectUri_003Ed__57(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
			{
				try
				{
					if (num == -4 || (uint)(num - 1) <= 1u)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003Cinvoker_003E5__3 = default(KeyValuePair<string, IUriInvokable>);
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				UIManager uIManager = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					LinkedPrefabs linkedPrefabs = uIManager.LinkedPrefabs;
					if (linkedPrefabs == null)
					{
						return false;
					}
					_003C_003E7__wrap1 = linkedPrefabs.GetUriInvokers().GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_0128;
				}
				case 1:
					_003C_003E1__state = -4;
					goto IL_0102;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_0102;
					}
					IL_0102:
					if (_003C_003E7__wrap3.MoveNext())
					{
						string current = _003C_003E7__wrap3.Current;
						if (string.IsNullOrEmpty(current))
						{
							_003C_003E2__current = _003Cinvoker_003E5__3.Key;
							_003C_003E1__state = 1;
							return true;
						}
						_003C_003E2__current = _003Cinvoker_003E5__3.Key + "/" + current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					_003Cinvoker_003E5__3 = default(KeyValuePair<string, IUriInvokable>);
					goto IL_0128;
					IL_0128:
					if (_003C_003E7__wrap1.MoveNext())
					{
						_003Cinvoker_003E5__3 = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap3 = _003Cinvoker_003E5__3.Value.CollectUri().GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_0102;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
				}
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CCollectUri_003Ed__57 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CCollectUri_003Ed__57(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	private static int _uiSize;

	public static bool UIInitializing;

	[SerializeField]
	public UIFont Font;

	private UIRoot _uiRoot;

	private LinkedPrefabs _linkedPrefabs;

	private JoystickGroup _virtualStick;

	private PopupGroup _popup;

	private AlarmGroup _alarm;

	private InventoryGroup _inventory;

	private MessageBox _messageBox;

	private bool _isInitScreenSize;

	private KeyboardHeightChecker _keyboardHeightChecker;

	private bool _isLoadingCurtain = true;

	public static int UISize
	{
		get
		{
			if (_uiSize == 0)
			{
				_uiSize = Platform.Instance.DefaultUISize;
			}
			return _uiSize;
		}
	}

	public static bool IsPortraitScreen { get; private set; }

	public static Rect SafeArea { get; private set; }

	public static bool IsLoadingCurtain => Singleton<UIManager>.Instance()._isLoadingCurtain;

	public static int ScreenWidth => Mathf.RoundToInt(NGUITools.screenSize.x * Singleton<UIManager>.Instance().UIRoot.pixelSizeAdjustment);

	public static int ScreenHeight => Singleton<UIManager>.Instance().UIRoot.activeHeight;

	public static int SafeWidth => (int)((float)ScreenWidth * SafeArea.width);

	public static int SafeHeight => (int)((float)ScreenHeight * SafeArea.height);

	private LinkedPrefabs LinkedPrefabs
	{
		get
		{
			if (_linkedPrefabs == null)
			{
				_linkedPrefabs = new LinkedPrefabs(UIRoot.gameObject, (!GameManager.IsPrologueMode) ? ResourceSingleton<UIPrefabMap>.Instance().GetMain() : ResourceSingleton<UIPrefabMap>.Instance().GetPrologue());
			}
			return _linkedPrefabs;
		}
	}

	public UIRoot UIRoot
	{
		get
		{
			if (_uiRoot == null)
			{
				GameObject gameObject = GameObject.Find("UI Root");
				if (gameObject != null)
				{
					_uiRoot = gameObject.GetComponent<UIRoot>();
				}
			}
			return _uiRoot;
		}
	}

	public JoystickGroup VirtualStick
	{
		get
		{
			if (_virtualStick == null)
			{
				_virtualStick = FindScript<JoystickGroup>();
			}
			return _virtualStick;
		}
	}

	public static PopupGroup Popup
	{
		get
		{
			if (!Singleton<UIManager>.HasInstance())
			{
				return null;
			}
			if (Singleton<UIManager>.Instance()._popup == null)
			{
				Singleton<UIManager>.Instance()._popup = FindScript<PopupGroup>();
			}
			return Singleton<UIManager>.Instance()._popup;
		}
	}

	public static AlarmGroup Alarm
	{
		get
		{
			if (!Singleton<UIManager>.HasInstance())
			{
				return null;
			}
			if (Singleton<UIManager>.Instance()._alarm == null)
			{
				Singleton<UIManager>.Instance()._alarm = FindScript<AlarmGroup>();
			}
			return Singleton<UIManager>.Instance()._alarm;
		}
	}

	public static InventoryGroup Inventory
	{
		get
		{
			if (!Singleton<UIManager>.HasInstance())
			{
				return null;
			}
			if (Singleton<UIManager>.Instance()._inventory == null)
			{
				Singleton<UIManager>.Instance()._inventory = FindScript<InventoryGroup>();
			}
			return Singleton<UIManager>.Instance()._inventory;
		}
	}

	public static MessageBox MessageBox
	{
		get
		{
			if (!Singleton<UIManager>.HasInstance())
			{
				return null;
			}
			if (Singleton<UIManager>.Instance()._messageBox == null)
			{
				Singleton<UIManager>.Instance()._messageBox = FindScript<MessageBox>();
			}
			return Singleton<UIManager>.Instance()._messageBox;
		}
	}

	public static event Action PreScreenResize;

	public static event Action ScreenResized;

	public static event Action ClosableUIChanged;

	int IUriInvokable.InvokeUri(string[] tokens, int start)
	{
		if (KUtility.GetSize(tokens) - start <= 0)
		{
			return 0;
		}
		LinkedPrefabs linkedPrefabs = LinkedPrefabs;
		if (linkedPrefabs == null)
		{
			return 0;
		}
		string key = tokens[start];
		IUriInvokable uriInvokable = linkedPrefabs.FindUriInvoker(key);
		if (uriInvokable == null)
		{
			return 0;
		}
		return 1 + uriInvokable.InvokeUri(tokens, start + 1);
	}

	public IEnumerable<string> CollectUri()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCollectUri_003Ed__57(-2)
		{
			_003C_003E4__this = this
		};
	}

	public static bool IsPortraitWidget(GameObject obj)
	{
		if (Platform.Instance.SupportPortrait)
		{
			return IsPortraitScreen;
		}
		UIBase uIBase = obj.GetComponent<UIBase>();
		if (uIBase == null)
		{
			uIBase = UIUtility.FindComponentInParent<UIBase>(obj);
		}
		if (uIBase != null)
		{
			return uIBase.IsPortrait;
		}
		return false;
	}

	protected override void OnAwake()
	{
		UIRoot.gameObject.AddComponent<UIRootAnchor>();
		InitUIGroups();
		OnScreenResize();
		Singleton<PlayerController>.Instance().MoveStarted += UIBase.OnPlayerMoveStart;
		Connections.Frontend.On(delegate(Announce msg, PacketHeader header)
		{
			if (Singleton<UIManager>.HasInstance())
			{
				ChatStruct chat = new ChatStruct
				{
					EntityId = "1000",
					Name = T._("[ffbf00]시스템[-]"),
					Body = new RadioNotice
					{
						Text = msg.Text
					},
					Type = ChannelType.System
				};
				GameSystem<SocialSystem>.Instance().AddChat(chat);
				SystemMsg(msg.Text, 4f);
			}
		});
		Connections.Frontend.On<Text>(TextPacketHandler);
		Connections.Radiotower.On<Text>(TextPacketHandler);
		Connections.Frontend.On<Messages.TimelineLog>(OnTimelineLog);
		OrientationController.LockRotation(OrientationController.RotationLock.Loading);
		OnLoadingCurtainHidden(delegate
		{
			_isLoadingCurtain = false;
			OrientationController.UnlockRotation(OrientationController.RotationLock.Loading);
		});
		_keyboardHeightChecker = new KeyboardHeightChecker();
	}

	private static void TextPacketHandler(Text msg, PacketHeader header)
	{
		SystemMsg(msg._Text, 4f);
	}

	private void Start()
	{
		UIBase.UIOpened += UIOpened;
		UIBase.UIClosed += UIClosed;
		UIBase.OnPreCloseUI += OnPreCloseUI;
		UIBase.OnScreenOrientationLock += OnScreenOrientationLock;
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(OnScreenResize));
		GameSystem<InputSystem>.Instance().On(InputCommand.InputUIFocusOut, InputUIFocusOutReceived);
		GameSystem<InputSystem>.Instance().On(InputCommand.Back, InputBackReceived);
		GameSystem<InputSystem>.Instance().On(InputCommand.ConfirmModalPopup, TryConfirmOnModal);
		GameSystem<InputSystem>.Instance().On(InputCommand.CancelModalPopup, TryCancelOnModal);
	}

	protected override void OnDestroyed()
	{
		UIManager.PreScreenResize = null;
		UIManager.ScreenResized = null;
		UIManager.ClosableUIChanged = null;
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Remove(UICamera.onScreenResize, new UICamera.OnScreenResize(OnScreenResize));
	}

	private void Update()
	{
		_keyboardHeightChecker.Check();
	}

	private void OnScreenResize()
	{
		Vector2 screenSize = NGUITools.screenSize;
		IsPortraitScreen = Platform.Instance.SupportPortrait && screenSize.x < screenSize.y;
		StartCoroutine(CoUpdateScreenSize());
	}

	private void InitUIGroups()
	{
		if (UIRoot == null)
		{
			return;
		}
		UIInitializing = true;
		LinkedPrefabs.Load(UIInitFunc, UIFilterFunc);
		UIInitializing = false;
		IUIInitializable[] componentsInChildren = UIRoot.GetComponentsInChildren<IUIInitializable>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			try
			{
				componentsInChildren[i].Init();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private static bool UIFilterFunc(GameObject obj)
	{
		if (!Debug.isDebugBuild)
		{
			string text = obj.name;
			if (text.Contains("Development") || text.Contains("CommandButton") || text.Contains("BuildCheat") || text.Contains("MakeCheat"))
			{
				return false;
			}
		}
		return true;
	}

	private static void UIInitFunc(GameObject obj)
	{
		UIBase component = obj.GetComponent<UIBase>();
		if (component != null)
		{
			component.Init(UIRootAnchor.GetRootAnchor(component.Anchor));
		}
	}

	private void RefreshAllUIAnchors()
	{
		if (UIRoot == null)
		{
			return;
		}
		UIRootAnchor.UpdateAndResetRootAnchors();
		Transform t = UIRoot.transform;
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(t);
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			UIRect component = transform.GetComponent<UIRect>();
			if (component != null)
			{
				component.ResetAndUpdateAnchors();
			}
			else
			{
				UIAnchor component2 = transform.GetComponent<UIAnchor>();
				if (component2 != null)
				{
					component2.Update();
				}
			}
			try
			{
				transform.GetComponent<IScreenResizeReceiver>()?.OnChangeScreenSize();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			int i = 0;
			for (int childCount = transform.childCount; i < childCount; i++)
			{
				stack.Push(transform.GetChild(i));
			}
		}
	}

	public Transform FindTransform(string fullPathName)
	{
		Transform obj = UIRoot.transform.Find(fullPathName);
		_ = obj == null;
		return obj;
	}

	public void HideUIRoot(bool hide, float duration = 1f)
	{
		TweenAlpha.Begin(UIRoot.gameObject, duration, (!hide) ? 1f : 0f);
	}

	public static void SetUISize(int size)
	{
		if (_uiSize != size)
		{
			_uiSize = size;
			if (Singleton<UIManager>.HasInstance())
			{
				Singleton<UIManager>.Instance().OnScreenResize();
			}
		}
	}

	private IEnumerator CoUpdateScreenSize()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoUpdateScreenSize_003Ed__72(0)
		{
			_003C_003E4__this = this
		};
	}

	private void OnScreenSizeChanged()
	{
		if (UIBase.IsLockScreenOrientation)
		{
			UIBase.CloseAllUI();
		}
		SafeArea = UIAnchorPolicy.Instance.GetSafeRect();
		UIAnchorPolicy.Instance.CalculateRootAnchors();
		if (UIManager.PreScreenResize != null)
		{
			UIManager.PreScreenResize();
		}
		RefreshAllUIAnchors();
		if (UIManager.ScreenResized != null)
		{
			UIManager.ScreenResized();
		}
	}

	private static void UIOpened()
	{
		OnChangeCloseableUI();
	}

	private static void UIClosed()
	{
		OnChangeCloseableUI();
	}

	private static void OnPreCloseUI(ref bool res)
	{
		if (res)
		{
			return;
		}
		MessageBox messageBox = MessageBox;
		if (messageBox != null && messageBox.IsShow)
		{
			messageBox.Hide(byBackButton: true);
			res = true;
			return;
		}
		if (TooltipBase.Close())
		{
			if (!Platform.Instance.UsePCUI)
			{
				res = true;
			}
			return;
		}
		InteractionGroup interactionGroup = FindScript<InteractionGroup>();
		if (interactionGroup != null && interactionGroup.InteractionMenu.CloseMenus())
		{
			res = true;
			return;
		}
		PrologueLeftMenuListGroupBase prologueLeftMenuListGroupBase = FindScript<PrologueLeftMenuListGroupBase>();
		if (prologueLeftMenuListGroupBase != null && prologueLeftMenuListGroupBase.IsOpened)
		{
			prologueLeftMenuListGroupBase.Close();
			res = true;
			return;
		}
		MenuListGroupBase menuListGroupBase = FindScript<MenuListGroupBase>();
		if (menuListGroupBase != null && menuListGroupBase.IsOpened)
		{
			menuListGroupBase.Close();
			res = true;
		}
	}

	private static void OnScreenOrientationLock()
	{
		if (UIBase.IsLockScreenOrientation)
		{
			OrientationController.LockRotation(OrientationController.RotationLock.UI);
		}
		else
		{
			OrientationController.UnlockRotation(OrientationController.RotationLock.UI);
		}
	}

	private void FullscreenAnchorTypeChanged(bool rightSideToFullscreen)
	{
		if (rightSideToFullscreen)
		{
			Singleton<CameraController>.Instance().ScreenOffset(Vector2.zero, 0.3f, NgInterpolate.EaseType.EaseOutCubic);
			return;
		}
		Vector3 vector = new Vector3(-360f / MainCamera.NGUIScale(), 0f, 0f);
		Singleton<CameraController>.Instance().ScreenOffset(vector, 0.3f, NgInterpolate.EaseType.EaseOutCubic);
	}

	private static void OnChangeCloseableUI()
	{
		bool flag = false;
		UIBase.AnchorType anchorType = UIBase.AnchorType.Default;
		if (UIBase.CurrentUI != null)
		{
			flag = UIBase.CurrentUI.GameBlur;
			anchorType = UIBase.CurrentUI.Anchor;
		}
		if (flag)
		{
			if (Platform.Instance.UsePCUI && anchorType == UIBase.AnchorType.FullscreenMobileOnly)
			{
				BlurController.BlurOn("Fullscreen", anchorType);
			}
			else
			{
				BlurController.BlurOn("Fullscreen", BlurController.Mask.Game);
			}
		}
		else
		{
			BlurController.BlurOff("Fullscreen");
		}
		if (UIManager.ClosableUIChanged != null)
		{
			UIManager.ClosableUIChanged();
		}
	}

	public void ToggleClickEventHandler(string fullPathName, UIEventListener.VoidDelegate handler, bool add)
	{
		Transform transform = FindTransform(fullPathName);
		if (!(transform == null))
		{
			UIEventListener uIEventListener = UIEventListener.Get(transform.gameObject);
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

	private void OnTimelineLog(Messages.TimelineLog msg, PacketHeader header)
	{
		new TimelineLogBuilder(msg).Build(delegate(TimelineLogBuilder timelineLog)
		{
			if (!string.IsNullOrEmpty(timelineLog.Text))
			{
				Alarm.ShowNotify(timelineLog.Text, "alarm_memo", major: true);
				GameSystem<SocialSystem>.Instance().AddSystemChat(timelineLog.Text, string.Empty, remainColor: true);
			}
		});
	}

	public static TV ShowLoadingCurtain<TV>() where TV : LoadingCurtainBase
	{
		LoadingCurtainGroup loadingCurtainGroup = FindScript<LoadingCurtainGroup>();
		if (loadingCurtainGroup == null)
		{
			return null;
		}
		return loadingCurtainGroup.Show<TV>();
	}

	public static void OnLoadingCurtainHidden(EventDelegate.Callback func, LoadingCurtainBase.LoadingState state = LoadingCurtainBase.LoadingState.Closed)
	{
		LoadingCurtainGroup loadingCurtainGroup = FindScript<LoadingCurtainGroup>();
		if (loadingCurtainGroup != null && loadingCurtainGroup.IsVisible)
		{
			List<EventDelegate> list = null;
			switch (state)
			{
			case LoadingCurtainBase.LoadingState.Open:
				list = loadingCurtainGroup.LoadingStarted;
				break;
			case LoadingCurtainBase.LoadingState.Closing:
				list = loadingCurtainGroup.FadeOutStarted;
				break;
			case LoadingCurtainBase.LoadingState.Closed:
				list = loadingCurtainGroup.FadeOutFinished;
				break;
			}
			if (list != null)
			{
				EventDelegate.Add(list, func, oneShot: true);
				return;
			}
		}
		func();
	}

	public static void AddOnScreenResized(Action func)
	{
		if (func != null)
		{
			ScreenResized += func;
			func();
		}
	}

	public static void AddOnPreScreenResize(Action func)
	{
		if (func != null)
		{
			PreScreenResize += func;
			func();
		}
	}

	public static TV FindScript<TV>() where TV : Component
	{
		if (!Singleton<UIManager>.Exist())
		{
			return null;
		}
		LinkedPrefabs linkedPrefabs = Singleton<UIManager>.Instance().LinkedPrefabs;
		if (linkedPrefabs != null)
		{
			return linkedPrefabs.FindScript<TV>();
		}
		return null;
	}

	public static TV Open<TV>() where TV : UIBase
	{
		TV val = FindScript<TV>();
		if (val == null)
		{
			return null;
		}
		val.Open();
		return val;
	}

	public static void ShowLoadingIcon(bool show)
	{
		if (Popup != null)
		{
			Popup.IsLoading = show;
		}
	}

	public static void SystemMsg(string comment, float duration = 3f)
	{
		SystemMsg(null, comment, duration);
	}

	public static void SystemMsg(string key, string comment, float duration = 3f)
	{
		if (Singleton<UIManager>.HasInstance())
		{
			AlarmGroup alarm = Alarm;
			if (alarm != null)
			{
				alarm.PushMessage(key, comment, duration);
			}
		}
	}

	public static void IgnoreUIDrag(GameObject go, Vector2 delta)
	{
		SetCurrentUITouchEvent(enable: false);
	}

	public static void SetCurrentUITouchEvent(bool enable)
	{
		if (!Singleton<PlayerController>.HasInstance())
		{
			return;
		}
		int touchCount = Input.touchCount;
		InputTouch.TouchEvent touchEvent;
		for (int i = 0; i < touchCount; i++)
		{
			touchEvent = GameSystem<InputSystem>.Instance().Touch.FindTouch(Input.GetTouch(i).fingerId);
			if (touchEvent != null)
			{
				touchEvent.IsNguiTouched = enable;
			}
		}
		touchEvent = GameSystem<InputSystem>.Instance().Touch.FindTouch(-10);
		if (touchEvent != null)
		{
			touchEvent.IsNguiTouched = enable;
		}
	}

	public static string ColorBBCode(Color c)
	{
		return "[" + NGUIText.EncodeColor(c) + "]";
	}

	private static void InputBackReceived(InputCommandMessage message)
	{
		if (UIBase.CloseUI())
		{
			GameSystem<InputSystem>.Instance().StopPropagation();
		}
		else if (MessageBox != null)
		{
			MessageBox.Show(T._("종료하시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					Platform.Instance.Quit();
				}
			});
		}
		else
		{
			Platform.Instance.Quit();
		}
	}

	private static void TryConfirmOnModal(InputCommandMessage message)
	{
		if (!MessageBox.IsShow)
		{
			TooltipBase.TryConfirmOnModal(message);
		}
	}

	private static void TryCancelOnModal(InputCommandMessage message)
	{
		if (!MessageBox.IsShow)
		{
			TooltipBase.TryCancelOnModal(message);
		}
	}

	private static void InputUIFocusOutReceived(InputCommandMessage message)
	{
		UIInput uIInput = ((!(UICamera.selectedObject != null)) ? null : UICamera.selectedObject.GetComponent<UIInput>());
		if (!(uIInput == null))
		{
			if (string.IsNullOrEmpty(uIInput.value))
			{
				uIInput.isSelected = false;
				return;
			}
			uIInput.value = string.Empty;
			uIInput.ShouldWaitFlushedInputString = true;
		}
	}

	private static void InputInventoryReceived(InputCommandMessage message)
	{
		if (!(Inventory == null))
		{
			if (Inventory.IsOpened)
			{
				Inventory.Close();
			}
			else
			{
				Inventory.Open();
			}
		}
	}
}
