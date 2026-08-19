using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Event System (UICamera)")]
public class UICamera : MonoBehaviour
{
	public enum ControlScheme
	{
		Mouse,
		Touch,
		Controller
	}

	public enum ClickNotification
	{
		None,
		Always,
		BasedOnDelta
	}

	public class MouseOrTouch
	{
		public KeyCode key;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 delta;

		public Vector2 totalDelta;

		public Camera pressedCam;

		public GameObject last;

		public GameObject current;

		public GameObject pressed;

		public GameObject dragged;

		public float pressTime;

		public float clickTime;

		public ClickNotification clickNotification = ClickNotification.Always;

		public bool touchBegan = true;

		public bool pressStarted;

		public bool dragStarted;

		public int ignoreDelta;

		public float deltaTime => RealTime.time - pressTime;

		public bool isOverUI => (Object)(object)current != (Object)null && (Object)(object)current != (Object)(object)fallThrough && (Object)(object)NGUITools.FindInParents<UIRoot>(current) != (Object)null;
	}

	public enum EventType
	{
		World_3D,
		UI_3D,
		World_2D,
		UI_2D
	}

	public enum ProcessEventsIn
	{
		Update,
		LateUpdate
	}

	private struct DepthEntry
	{
		public int depth;

		public RaycastHit hit;

		public Vector3 point;

		public GameObject go;
	}

	public class Touch
	{
		public int fingerId;

		public TouchPhase phase;

		public Vector2 position;

		public int tapCount;
	}

	public delegate bool GetKeyStateFunc(KeyCode key);

	public delegate float GetAxisFunc(string name);

	public delegate bool GetAnyKeyFunc();

	public delegate void OnScreenResize();

	public delegate void OnCustomInput();

	public delegate void OnSchemeChange();

	public delegate void MoveDelegate(Vector2 delta);

	public delegate void VoidDelegate(GameObject go);

	public delegate void BoolDelegate(GameObject go, bool state);

	public delegate void FloatDelegate(GameObject go, float delta);

	public delegate void VectorDelegate(GameObject go, Vector2 delta);

	public delegate void ObjectDelegate(GameObject go, GameObject obj);

	public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

	public delegate int GetTouchCountCallback();

	public delegate Touch GetTouchCallback(int index);

	public static BetterList<UICamera> list = new BetterList<UICamera>();

	public static GetKeyStateFunc GetKeyDown = (KeyCode key) => ((int)key < 330 || !ignoreControllerInput) && Input.GetKeyDown(key);

	public static GetKeyStateFunc GetKeyUp = (KeyCode key) => ((int)key < 330 || !ignoreControllerInput) && Input.GetKeyUp(key);

	public static GetKeyStateFunc GetKey = (KeyCode key) => ((int)key < 330 || !ignoreControllerInput) && Input.GetKey(key);

	public static GetAxisFunc GetAxis = (string axis) => ignoreControllerInput ? 0f : Input.GetAxis(axis);

	public static GetAnyKeyFunc GetAnyKeyDown;

	public static OnScreenResize onScreenResize;

	public EventType eventType = EventType.UI_3D;

	public bool eventsGoToColliders;

	public LayerMask eventReceiverMask = LayerMask.op_Implicit(-1);

	public ProcessEventsIn processEventsIn;

	public bool debug;

	public bool useMouse = true;

	public bool useTouch = true;

	public bool allowMultiTouch = true;

	public bool useKeyboard = true;

	public bool useController = true;

	public bool stickyTooltip = true;

	public float tooltipDelay = 1f;

	public bool longPressTooltip;

	public float mouseDragThreshold = 4f;

	public float mouseClickThreshold = 10f;

	public float touchDragThreshold = 40f;

	public float touchClickThreshold = 40f;

	public float rangeDistance = -1f;

	public string horizontalAxisName = "Horizontal";

	public string verticalAxisName = "Vertical";

	public string horizontalPanAxisName;

	public string verticalPanAxisName;

	public string scrollAxisName = "Mouse ScrollWheel";

	public bool commandClick = true;

	public KeyCode submitKey0 = (KeyCode)13;

	public KeyCode submitKey1 = (KeyCode)330;

	public KeyCode cancelKey0 = (KeyCode)27;

	public KeyCode cancelKey1 = (KeyCode)331;

	public bool autoHideCursor = true;

	public static OnCustomInput onCustomInput;

	public static bool showTooltips = true;

	public static bool ignoreControllerInput = false;

	private static bool mDisableController = false;

	private static Vector2 mLastPos = Vector2.zero;

	public static Vector3 lastWorldPosition = Vector3.zero;

	public static RaycastHit lastHit;

	public static UICamera current = null;

	public static Camera currentCamera = null;

	public static OnSchemeChange onSchemeChange;

	private static ControlScheme mLastScheme = ControlScheme.Mouse;

	public static int currentTouchID = -100;

	private static KeyCode mCurrentKey = (KeyCode)48;

	public static MouseOrTouch currentTouch = null;

	private static bool mInputFocus = false;

	private static GameObject mGenericHandler;

	public static GameObject fallThrough;

	public static VoidDelegate onClick;

	public static VoidDelegate onDoubleClick;

	public static BoolDelegate onHover;

	public static BoolDelegate onPress;

	public static VoidDelegate onLongPress;

	public static BoolDelegate onSelect;

	public static FloatDelegate onScroll;

	public static VectorDelegate onDrag;

	public static VoidDelegate onDragStart;

	public static ObjectDelegate onDragOver;

	public static ObjectDelegate onDragOut;

	public static VoidDelegate onDragEnd;

	public static ObjectDelegate onDrop;

	public static KeyCodeDelegate onKey;

	public static KeyCodeDelegate onNavigate;

	public static VectorDelegate onPan;

	public static BoolDelegate onTooltip;

	public static MoveDelegate onMouseMove;

	public static Action<MouseOrTouch> onPostRaycast;

	private static MouseOrTouch[] mMouse = new MouseOrTouch[3]
	{
		new MouseOrTouch(),
		new MouseOrTouch(),
		new MouseOrTouch()
	};

	public static MouseOrTouch controller = new MouseOrTouch();

	public static List<MouseOrTouch> activeTouches = new List<MouseOrTouch>();

	private static List<int> mTouchIDs = new List<int>();

	private static int mWidth = 0;

	private static int mHeight = 0;

	private static GameObject mTooltip = null;

	private Camera mCam;

	private static float mTooltipTime = 0f;

	private float mNextRaycast;

	public static bool isDragging = false;

	private static GameObject mRayHitObject;

	private static GameObject mHover;

	private static GameObject mSelected;

	private static DepthEntry mHit = default(DepthEntry);

	private static BetterList<DepthEntry> mHits = new BetterList<DepthEntry>();

	private static Plane m2DPlane = new Plane(Vector3.back, 0f);

	private static float mNextEvent = 0f;

	private static int mNotifying = 0;

	private static bool mUsingTouchEvents = true;

	public static GetTouchCountCallback GetInputTouchCount;

	public static GetTouchCallback GetInputTouch;

	[Obsolete("Use new OnDragStart / OnDragOver / OnDragOut / OnDragEnd events instead")]
	public bool stickyPress => true;

	public static bool disableController
	{
		get
		{
			return mDisableController && !UIPopupList.isOpen;
		}
		set
		{
			mDisableController = value;
		}
	}

	[Obsolete("Use lastEventPosition instead. It handles controller input properly.")]
	public static Vector2 lastTouchPosition
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return mLastPos;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			mLastPos = value;
		}
	}

	public static Vector2 lastEventPosition
	{
		get
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			ControlScheme controlScheme = currentScheme;
			if (controlScheme == ControlScheme.Controller)
			{
				GameObject val = hoveredObject;
				if ((Object)(object)val != (Object)null)
				{
					Bounds val2 = NGUIMath.CalculateAbsoluteWidgetBounds(val.transform);
					Camera val3 = NGUITools.FindCameraForLayer(val.layer);
					return Vector2.op_Implicit(val3.WorldToScreenPoint(((Bounds)(ref val2)).center));
				}
			}
			return mLastPos;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			mLastPos = value;
		}
	}

	public static UICamera first
	{
		get
		{
			if (list == null || list.size == 0)
			{
				return null;
			}
			return list[0];
		}
	}

	public static ControlScheme currentScheme
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Invalid comparison between Unknown and I4
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			if ((int)mCurrentKey == 0)
			{
				return ControlScheme.Touch;
			}
			if ((int)mCurrentKey >= 330)
			{
				return ControlScheme.Controller;
			}
			if ((Object)(object)current != (Object)null && mLastScheme == ControlScheme.Controller && (mCurrentKey == current.submitKey0 || mCurrentKey == current.submitKey1))
			{
				return ControlScheme.Controller;
			}
			return ControlScheme.Mouse;
		}
		set
		{
			switch (value)
			{
			case ControlScheme.Mouse:
				currentKey = (KeyCode)323;
				break;
			case ControlScheme.Controller:
				currentKey = (KeyCode)330;
				break;
			case ControlScheme.Touch:
				currentKey = (KeyCode)0;
				break;
			default:
				currentKey = (KeyCode)48;
				break;
			}
			mLastScheme = value;
		}
	}

	public static KeyCode currentKey
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return mCurrentKey;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (mCurrentKey == value)
			{
				return;
			}
			ControlScheme controlScheme = mLastScheme;
			mCurrentKey = value;
			mLastScheme = currentScheme;
			if (controlScheme != mLastScheme)
			{
				HideTooltip();
				if (mLastScheme == ControlScheme.Mouse)
				{
					Cursor.lockState = (CursorLockMode)0;
					Cursor.visible = true;
				}
				else if ((Object)(object)current != (Object)null && current.autoHideCursor)
				{
					Cursor.visible = false;
					Cursor.lockState = (CursorLockMode)1;
					mMouse[0].ignoreDelta = 2;
				}
				if (onSchemeChange != null)
				{
					onSchemeChange();
				}
			}
		}
	}

	public static Ray currentRay => (Ray)((!((Object)(object)currentCamera != (Object)null) || currentTouch == null) ? default(Ray) : currentCamera.ScreenPointToRay(Vector2.op_Implicit(currentTouch.pos)));

	public static bool inputHasFocus
	{
		get
		{
			if (mInputFocus && Object.op_Implicit((Object)(object)mSelected) && mSelected.activeInHierarchy)
			{
				return true;
			}
			return false;
		}
	}

	[Obsolete("Use delegates instead such as UICamera.onClick, UICamera.onHover, etc.")]
	public static GameObject genericEventHandler
	{
		get
		{
			return mGenericHandler;
		}
		set
		{
			mGenericHandler = value;
		}
	}

	private bool handlesEvents => (Object)(object)eventHandler == (Object)(object)this;

	public Camera cachedCamera
	{
		get
		{
			if ((Object)(object)mCam == (Object)null)
			{
				mCam = ((Component)this).GetComponent<Camera>();
			}
			return mCam;
		}
	}

	public static GameObject tooltipObject => mTooltip;

	public static bool isOverUI
	{
		get
		{
			if (currentTouch != null)
			{
				return currentTouch.isOverUI;
			}
			int i = 0;
			for (int count = activeTouches.Count; i < count; i++)
			{
				MouseOrTouch mouseOrTouch = activeTouches[i];
				if ((Object)(object)mouseOrTouch.pressed != (Object)null && (Object)(object)mouseOrTouch.pressed != (Object)(object)fallThrough && (Object)(object)NGUITools.FindInParents<UIRoot>(mouseOrTouch.pressed) != (Object)null)
				{
					return true;
				}
			}
			if ((Object)(object)mMouse[0].current != (Object)null && (Object)(object)mMouse[0].current != (Object)(object)fallThrough && (Object)(object)NGUITools.FindInParents<UIRoot>(mMouse[0].current) != (Object)null)
			{
				return true;
			}
			if ((Object)(object)controller.pressed != (Object)null && (Object)(object)controller.pressed != (Object)(object)fallThrough && (Object)(object)NGUITools.FindInParents<UIRoot>(controller.pressed) != (Object)null)
			{
				return true;
			}
			return false;
		}
	}

	public static GameObject hoveredObject
	{
		get
		{
			if (currentTouch != null && currentTouch.dragStarted)
			{
				return currentTouch.current;
			}
			if (Object.op_Implicit((Object)(object)mHover) && mHover.activeInHierarchy)
			{
				return mHover;
			}
			mHover = null;
			return null;
		}
		set
		{
			if ((Object)(object)mHover == (Object)(object)value)
			{
				return;
			}
			bool flag = false;
			UICamera uICamera = current;
			if (currentTouch == null)
			{
				flag = true;
				currentTouchID = -100;
				currentTouch = controller;
			}
			ShowTooltip(null);
			if (Object.op_Implicit((Object)(object)mSelected) && currentScheme == ControlScheme.Controller)
			{
				Notify(mSelected, "OnSelect", false);
				if (onSelect != null)
				{
					onSelect(mSelected, state: false);
				}
				mSelected = null;
			}
			if (Object.op_Implicit((Object)(object)mHover))
			{
				Notify(mHover, "OnHover", false);
				if (onHover != null)
				{
					onHover(mHover, state: false);
				}
			}
			mHover = value;
			currentTouch.clickNotification = ClickNotification.None;
			if (Object.op_Implicit((Object)(object)mHover))
			{
				if ((Object)(object)mHover != (Object)(object)controller.current && (Object)(object)mHover.GetComponent<UIKeyNavigation>() != (Object)null)
				{
					controller.current = mHover;
				}
				if (flag)
				{
					UICamera uICamera2 = ((!((Object)(object)mHover != (Object)null)) ? list[0] : FindCameraForLayer(mHover.layer));
					if ((Object)(object)uICamera2 != (Object)null)
					{
						current = uICamera2;
						currentCamera = uICamera2.cachedCamera;
					}
				}
				if (onHover != null)
				{
					onHover(mHover, state: true);
				}
				Notify(mHover, "OnHover", true);
			}
			if (flag)
			{
				current = uICamera;
				currentCamera = ((!((Object)(object)uICamera != (Object)null)) ? null : uICamera.cachedCamera);
				currentTouch = null;
				currentTouchID = -100;
			}
		}
	}

	public static GameObject controllerNavigationObject
	{
		get
		{
			if (Object.op_Implicit((Object)(object)controller.current) && controller.current.activeInHierarchy)
			{
				return controller.current;
			}
			if (currentScheme == ControlScheme.Controller && (Object)(object)current != (Object)null && current.useController && !ignoreControllerInput && UIKeyNavigation.list.size > 0)
			{
				for (int i = 0; i < UIKeyNavigation.list.size; i++)
				{
					UIKeyNavigation uIKeyNavigation = UIKeyNavigation.list[i];
					if (Object.op_Implicit((Object)(object)uIKeyNavigation) && uIKeyNavigation.constraint != UIKeyNavigation.Constraint.Explicit && uIKeyNavigation.startsSelected)
					{
						hoveredObject = ((Component)uIKeyNavigation).gameObject;
						controller.current = mHover;
						return mHover;
					}
				}
				if ((Object)(object)mHover == (Object)null)
				{
					for (int j = 0; j < UIKeyNavigation.list.size; j++)
					{
						UIKeyNavigation uIKeyNavigation2 = UIKeyNavigation.list[j];
						if (Object.op_Implicit((Object)(object)uIKeyNavigation2) && uIKeyNavigation2.constraint != UIKeyNavigation.Constraint.Explicit)
						{
							hoveredObject = ((Component)uIKeyNavigation2).gameObject;
							controller.current = mHover;
							return mHover;
						}
					}
				}
			}
			controller.current = null;
			return null;
		}
		set
		{
			if ((Object)(object)controller.current != (Object)(object)value && Object.op_Implicit((Object)(object)controller.current))
			{
				Notify(controller.current, "OnHover", false);
				if (onHover != null)
				{
					onHover(controller.current, state: false);
				}
				controller.current = null;
			}
			hoveredObject = value;
		}
	}

	public static GameObject selectedObject
	{
		get
		{
			if (Object.op_Implicit((Object)(object)mSelected) && mSelected.activeInHierarchy)
			{
				return mSelected;
			}
			mSelected = null;
			return null;
		}
		set
		{
			if ((Object)(object)mSelected == (Object)(object)value)
			{
				hoveredObject = value;
				controller.current = value;
				return;
			}
			ShowTooltip(null);
			bool flag = false;
			UICamera uICamera = current;
			if (currentTouch == null)
			{
				flag = true;
				currentTouchID = -100;
				currentTouch = controller;
			}
			mInputFocus = false;
			if (Object.op_Implicit((Object)(object)mSelected))
			{
				Notify(mSelected, "OnSelect", false);
				if (onSelect != null)
				{
					onSelect(mSelected, state: false);
				}
			}
			mSelected = value;
			currentTouch.clickNotification = ClickNotification.None;
			if ((Object)(object)value != (Object)null)
			{
				UIKeyNavigation component = value.GetComponent<UIKeyNavigation>();
				if ((Object)(object)component != (Object)null)
				{
					controller.current = value;
				}
			}
			if (Object.op_Implicit((Object)(object)mSelected) && flag)
			{
				UICamera uICamera2 = ((!((Object)(object)mSelected != (Object)null)) ? list[0] : FindCameraForLayer(mSelected.layer));
				if ((Object)(object)uICamera2 != (Object)null)
				{
					current = uICamera2;
					currentCamera = uICamera2.cachedCamera;
				}
			}
			if (Object.op_Implicit((Object)(object)mSelected))
			{
				mInputFocus = mSelected.activeInHierarchy && (Object)(object)mSelected.GetComponent<UIInput>() != (Object)null;
				if (onSelect != null)
				{
					onSelect(mSelected, state: true);
				}
				Notify(mSelected, "OnSelect", true);
			}
			if (flag)
			{
				current = uICamera;
				currentCamera = ((!((Object)(object)uICamera != (Object)null)) ? null : uICamera.cachedCamera);
				currentTouch = null;
				currentTouchID = -100;
			}
		}
	}

	[Obsolete("Use either 'CountInputSources()' or 'activeTouches.Count'")]
	public static int touchCount => CountInputSources();

	public static int dragCount
	{
		get
		{
			int num = 0;
			int i = 0;
			for (int count = activeTouches.Count; i < count; i++)
			{
				MouseOrTouch mouseOrTouch = activeTouches[i];
				if ((Object)(object)mouseOrTouch.dragged != (Object)null)
				{
					num++;
				}
			}
			for (int j = 0; j < mMouse.Length; j++)
			{
				if ((Object)(object)mMouse[j].dragged != (Object)null)
				{
					num++;
				}
			}
			if ((Object)(object)controller.dragged != (Object)null)
			{
				num++;
			}
			return num;
		}
	}

	public static Camera mainCamera
	{
		get
		{
			UICamera uICamera = eventHandler;
			return (!((Object)(object)uICamera != (Object)null)) ? null : uICamera.cachedCamera;
		}
	}

	public static UICamera eventHandler
	{
		get
		{
			for (int i = 0; i < list.size; i++)
			{
				UICamera uICamera = list.buffer[i];
				if (!((Object)(object)uICamera == (Object)null) && ((Behaviour)uICamera).enabled && NGUITools.GetActive(((Component)uICamera).gameObject))
				{
					return uICamera;
				}
			}
			return null;
		}
	}

	public static bool IsPressed(GameObject go)
	{
		for (int i = 0; i < 3; i++)
		{
			if ((Object)(object)mMouse[i].pressed == (Object)(object)go)
			{
				return true;
			}
		}
		int j = 0;
		for (int count = activeTouches.Count; j < count; j++)
		{
			MouseOrTouch mouseOrTouch = activeTouches[j];
			if ((Object)(object)mouseOrTouch.pressed == (Object)(object)go)
			{
				return true;
			}
		}
		if ((Object)(object)controller.pressed == (Object)(object)go)
		{
			return true;
		}
		return false;
	}

	public static int CountInputSources()
	{
		int num = 0;
		int i = 0;
		for (int count = activeTouches.Count; i < count; i++)
		{
			MouseOrTouch mouseOrTouch = activeTouches[i];
			if ((Object)(object)mouseOrTouch.pressed != (Object)null)
			{
				num++;
			}
		}
		for (int j = 0; j < mMouse.Length; j++)
		{
			if ((Object)(object)mMouse[j].pressed != (Object)null)
			{
				num++;
			}
		}
		if ((Object)(object)controller.pressed != (Object)null)
		{
			num++;
		}
		return num;
	}

	private static int CompareFunc(UICamera a, UICamera b)
	{
		if (a.cachedCamera.depth < b.cachedCamera.depth)
		{
			return 1;
		}
		if (a.cachedCamera.depth > b.cachedCamera.depth)
		{
			return -1;
		}
		return 0;
	}

	private static Rigidbody FindRootRigidbody(Transform trans)
	{
		while ((Object)(object)trans != (Object)null)
		{
			if ((Object)(object)((Component)trans).GetComponent<UIPanel>() != (Object)null)
			{
				return null;
			}
			Rigidbody component = ((Component)trans).GetComponent<Rigidbody>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
			trans = trans.parent;
		}
		return null;
	}

	private static Rigidbody2D FindRootRigidbody2D(Transform trans)
	{
		while ((Object)(object)trans != (Object)null)
		{
			if ((Object)(object)((Component)trans).GetComponent<UIPanel>() != (Object)null)
			{
				return null;
			}
			Rigidbody2D component = ((Component)trans).GetComponent<Rigidbody2D>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
			trans = trans.parent;
		}
		return null;
	}

	public static void Raycast(MouseOrTouch touch)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!Raycast(Vector2.op_Implicit(touch.pos)))
		{
			mRayHitObject = fallThrough;
		}
		if ((Object)(object)mRayHitObject == (Object)null)
		{
			mRayHitObject = mGenericHandler;
		}
		touch.last = touch.current;
		touch.current = mRayHitObject;
		mLastPos = touch.pos;
		if (onPostRaycast != null)
		{
			onPostRaycast(touch);
		}
	}

	public static bool Raycast(Vector3 inPos)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0770: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Unknown result type (might be due to invalid IL or missing references)
		//IL_061c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0621: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < list.size; i++)
		{
			UICamera uICamera = list.buffer[i];
			if (!((Behaviour)uICamera).enabled || !NGUITools.GetActive(((Component)uICamera).gameObject))
			{
				continue;
			}
			currentCamera = uICamera.cachedCamera;
			Vector3 val = currentCamera.ScreenToViewportPoint(inPos);
			if (float.IsNaN(val.x) || float.IsNaN(val.y))
			{
				continue;
			}
			Ray val2 = currentCamera.ScreenPointToRay(inPos);
			int num = currentCamera.cullingMask & LayerMask.op_Implicit(uICamera.eventReceiverMask);
			float num2 = ((!(uICamera.rangeDistance > 0f)) ? (currentCamera.farClipPlane - currentCamera.nearClipPlane) : uICamera.rangeDistance);
			if (uICamera.eventType == EventType.World_3D)
			{
				if (!Physics.Raycast(val2, ref lastHit, num2, num))
				{
					continue;
				}
				lastWorldPosition = ((RaycastHit)(ref lastHit)).point;
				mRayHitObject = ((Component)((RaycastHit)(ref lastHit)).collider).gameObject;
				if (!uICamera.eventsGoToColliders)
				{
					Rigidbody val3 = FindRootRigidbody(mRayHitObject.transform);
					if ((Object)(object)val3 != (Object)null)
					{
						mRayHitObject = ((Component)val3).gameObject;
					}
				}
				return true;
			}
			if (uICamera.eventType == EventType.UI_3D)
			{
				RaycastHit[] array = Physics.RaycastAll(val2, num2, num);
				if (array.Length > 1)
				{
					for (int j = 0; j < array.Length; j++)
					{
						GameObject gameObject = ((Component)((RaycastHit)(ref array[j])).collider).gameObject;
						UIWidget component = gameObject.GetComponent<UIWidget>();
						if ((Object)(object)component != (Object)null)
						{
							if (!component.isVisible || (component.hitCheck != null && !component.hitCheck(((RaycastHit)(ref array[j])).point)))
							{
								continue;
							}
						}
						else
						{
							UIRect uIRect = NGUITools.FindInParents<UIRect>(gameObject);
							if ((Object)(object)uIRect != (Object)null && uIRect.finalAlpha < 0.001f)
							{
								continue;
							}
						}
						mHit.depth = NGUITools.CalculateRaycastDepth(gameObject);
						if (mHit.depth != int.MaxValue)
						{
							mHit.hit = array[j];
							mHit.point = ((RaycastHit)(ref array[j])).point;
							mHit.go = ((Component)((RaycastHit)(ref array[j])).collider).gameObject;
							mHits.Add(mHit);
						}
					}
					mHits.Sort((DepthEntry r1, DepthEntry r2) => r2.depth.CompareTo(r1.depth));
					for (int k = 0; k < mHits.size; k++)
					{
						if (IsVisible(ref mHits.buffer[k]))
						{
							lastHit = mHits[k].hit;
							mRayHitObject = mHits[k].go;
							lastWorldPosition = mHits[k].point;
							mHits.Clear();
							return true;
						}
					}
					mHits.Clear();
				}
				else
				{
					if (array.Length != 1)
					{
						continue;
					}
					GameObject gameObject2 = ((Component)((RaycastHit)(ref array[0])).collider).gameObject;
					UIWidget component2 = gameObject2.GetComponent<UIWidget>();
					if ((Object)(object)component2 != (Object)null)
					{
						if (!component2.isVisible || (component2.hitCheck != null && !component2.hitCheck(((RaycastHit)(ref array[0])).point)))
						{
							continue;
						}
					}
					else
					{
						UIRect uIRect2 = NGUITools.FindInParents<UIRect>(gameObject2);
						if ((Object)(object)uIRect2 != (Object)null && uIRect2.finalAlpha < 0.001f)
						{
							continue;
						}
					}
					if (IsVisible(((RaycastHit)(ref array[0])).point, ((Component)((RaycastHit)(ref array[0])).collider).gameObject))
					{
						lastHit = array[0];
						lastWorldPosition = ((RaycastHit)(ref array[0])).point;
						mRayHitObject = ((Component)((RaycastHit)(ref lastHit)).collider).gameObject;
						return true;
					}
				}
			}
			else
			{
				if (uICamera.eventType == EventType.World_2D)
				{
					if (!((Plane)(ref m2DPlane)).Raycast(val2, ref num2))
					{
						continue;
					}
					Vector3 point = ((Ray)(ref val2)).GetPoint(num2);
					Collider2D val4 = Physics2D.OverlapPoint(Vector2.op_Implicit(point), num);
					if (!Object.op_Implicit((Object)(object)val4))
					{
						continue;
					}
					lastWorldPosition = point;
					mRayHitObject = ((Component)val4).gameObject;
					if (!uICamera.eventsGoToColliders)
					{
						Rigidbody2D val5 = FindRootRigidbody2D(mRayHitObject.transform);
						if ((Object)(object)val5 != (Object)null)
						{
							mRayHitObject = ((Component)val5).gameObject;
						}
					}
					return true;
				}
				if (uICamera.eventType != EventType.UI_2D || !((Plane)(ref m2DPlane)).Raycast(val2, ref num2))
				{
					continue;
				}
				lastWorldPosition = ((Ray)(ref val2)).GetPoint(num2);
				Collider2D[] array2 = Physics2D.OverlapPointAll(Vector2.op_Implicit(lastWorldPosition), num);
				if (array2.Length > 1)
				{
					for (int l = 0; l < array2.Length; l++)
					{
						GameObject gameObject3 = ((Component)array2[l]).gameObject;
						UIWidget component3 = gameObject3.GetComponent<UIWidget>();
						if ((Object)(object)component3 != (Object)null)
						{
							if (!component3.isVisible || (component3.hitCheck != null && !component3.hitCheck(lastWorldPosition)))
							{
								continue;
							}
						}
						else
						{
							UIRect uIRect3 = NGUITools.FindInParents<UIRect>(gameObject3);
							if ((Object)(object)uIRect3 != (Object)null && uIRect3.finalAlpha < 0.001f)
							{
								continue;
							}
						}
						mHit.depth = NGUITools.CalculateRaycastDepth(gameObject3);
						if (mHit.depth != int.MaxValue)
						{
							mHit.go = gameObject3;
							mHit.point = lastWorldPosition;
							mHits.Add(mHit);
						}
					}
					mHits.Sort((DepthEntry r1, DepthEntry r2) => r2.depth.CompareTo(r1.depth));
					for (int m = 0; m < mHits.size; m++)
					{
						if (IsVisible(ref mHits.buffer[m]))
						{
							mRayHitObject = mHits[m].go;
							mHits.Clear();
							return true;
						}
					}
					mHits.Clear();
				}
				else
				{
					if (array2.Length != 1)
					{
						continue;
					}
					GameObject gameObject4 = ((Component)array2[0]).gameObject;
					UIWidget component4 = gameObject4.GetComponent<UIWidget>();
					if ((Object)(object)component4 != (Object)null)
					{
						if (!component4.isVisible || (component4.hitCheck != null && !component4.hitCheck(lastWorldPosition)))
						{
							continue;
						}
					}
					else
					{
						UIRect uIRect4 = NGUITools.FindInParents<UIRect>(gameObject4);
						if ((Object)(object)uIRect4 != (Object)null && uIRect4.finalAlpha < 0.001f)
						{
							continue;
						}
					}
					if (IsVisible(lastWorldPosition, gameObject4))
					{
						mRayHitObject = gameObject4;
						return true;
					}
				}
			}
		}
		return false;
	}

	private static bool IsVisible(Vector3 worldPoint, GameObject go)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(go);
		while ((Object)(object)uIPanel != (Object)null)
		{
			if (!uIPanel.IsVisible(worldPoint))
			{
				return false;
			}
			uIPanel = uIPanel.parentPanel;
		}
		return true;
	}

	private static bool IsVisible(ref DepthEntry de)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(de.go);
		while ((Object)(object)uIPanel != (Object)null)
		{
			if (!uIPanel.IsVisible(de.point))
			{
				return false;
			}
			uIPanel = uIPanel.parentPanel;
		}
		return true;
	}

	public static bool IsHighlighted(GameObject go)
	{
		return (Object)(object)hoveredObject == (Object)(object)go;
	}

	public static UICamera FindCameraForLayer(int layer)
	{
		int num = 1 << layer;
		for (int i = 0; i < list.size; i++)
		{
			UICamera uICamera = list.buffer[i];
			Camera val = uICamera.cachedCamera;
			if ((Object)(object)val != (Object)null && (val.cullingMask & num) != 0)
			{
				return uICamera;
			}
		}
		return null;
	}

	private static int GetDirection(KeyCode up, KeyCode down)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (GetKeyDown(up))
		{
			currentKey = up;
			return 1;
		}
		if (GetKeyDown(down))
		{
			currentKey = down;
			return -1;
		}
		return 0;
	}

	private static int GetDirection(KeyCode up0, KeyCode up1, KeyCode down0, KeyCode down1)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (GetKeyDown(up0))
		{
			currentKey = up0;
			return 1;
		}
		if (GetKeyDown(up1))
		{
			currentKey = up1;
			return 1;
		}
		if (GetKeyDown(down0))
		{
			currentKey = down0;
			return -1;
		}
		if (GetKeyDown(down1))
		{
			currentKey = down1;
			return -1;
		}
		return 0;
	}

	private static int GetDirection(string axis)
	{
		float time = RealTime.time;
		if (mNextEvent < time && !string.IsNullOrEmpty(axis))
		{
			float num = GetAxis(axis);
			if (num > 0.75f)
			{
				currentKey = (KeyCode)330;
				mNextEvent = time + 0.25f;
				return 1;
			}
			if (num < -0.75f)
			{
				currentKey = (KeyCode)330;
				mNextEvent = time + 0.25f;
				return -1;
			}
		}
		return 0;
	}

	public static void Notify(GameObject go, string funcName, object obj)
	{
		if (mNotifying > 10)
		{
			return;
		}
		if (currentScheme == ControlScheme.Controller && UIPopupList.isOpen && (Object)(object)UIPopupList.current.source == (Object)(object)go && UIPopupList.isOpen)
		{
			go = ((Component)UIPopupList.current).gameObject;
		}
		if (Object.op_Implicit((Object)(object)go) && go.activeInHierarchy)
		{
			mNotifying++;
			go.SendMessage(funcName, obj, (SendMessageOptions)1);
			if ((Object)(object)mGenericHandler != (Object)null && (Object)(object)mGenericHandler != (Object)(object)go)
			{
				mGenericHandler.SendMessage(funcName, obj, (SendMessageOptions)1);
			}
			mNotifying--;
		}
	}

	public static MouseOrTouch GetMouse(int button)
	{
		return mMouse[button];
	}

	public static MouseOrTouch GetTouch(int id, bool createIfMissing = false)
	{
		if (id < 0)
		{
			return GetMouse(-id - 1);
		}
		int i = 0;
		for (int count = mTouchIDs.Count; i < count; i++)
		{
			if (mTouchIDs[i] == id)
			{
				return activeTouches[i];
			}
		}
		if (createIfMissing)
		{
			MouseOrTouch mouseOrTouch = new MouseOrTouch();
			mouseOrTouch.pressTime = RealTime.time;
			mouseOrTouch.touchBegan = true;
			activeTouches.Add(mouseOrTouch);
			mTouchIDs.Add(id);
			return mouseOrTouch;
		}
		return null;
	}

	public static void RemoveTouch(int id)
	{
		int i = 0;
		for (int count = mTouchIDs.Count; i < count; i++)
		{
			if (mTouchIDs[i] == id)
			{
				mTouchIDs.RemoveAt(i);
				activeTouches.RemoveAt(i);
				break;
			}
		}
	}

	private void Awake()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		mWidth = Screen.width;
		mHeight = Screen.height;
		currentScheme = ControlScheme.Touch;
		mMouse[0].pos = Vector2.op_Implicit(Input.mousePosition);
		for (int i = 1; i < 3; i++)
		{
			mMouse[i].pos = mMouse[0].pos;
			mMouse[i].lastPos = mMouse[0].pos;
		}
		mLastPos = mMouse[0].pos;
	}

	private void OnEnable()
	{
		list.Add(this);
		list.Sort(CompareFunc);
	}

	private void OnDisable()
	{
		list.Remove(this);
	}

	private void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		if (eventType != 0 && (int)cachedCamera.transparencySortMode != 2)
		{
			cachedCamera.transparencySortMode = (TransparencySortMode)2;
		}
		if (Application.isPlaying)
		{
			if ((Object)(object)fallThrough == (Object)null)
			{
				UIRoot uIRoot = NGUITools.FindInParents<UIRoot>(((Component)this).gameObject);
				fallThrough = ((!((Object)(object)uIRoot != (Object)null)) ? ((Component)this).gameObject : ((Component)uIRoot).gameObject);
			}
			cachedCamera.eventMask = 0;
		}
	}

	private void Update()
	{
		if (handlesEvents && processEventsIn == ProcessEventsIn.Update)
		{
			ProcessEvents();
		}
	}

	private void LateUpdate()
	{
		if (!handlesEvents)
		{
			return;
		}
		if (processEventsIn == ProcessEventsIn.LateUpdate)
		{
			ProcessEvents();
		}
		int width = Screen.width;
		int height = Screen.height;
		if (width != mWidth || height != mHeight)
		{
			mWidth = width;
			mHeight = height;
			UIRoot.Broadcast("UpdateAnchors");
			if (onScreenResize != null)
			{
				onScreenResize();
			}
		}
	}

	private void ProcessEvents()
	{
		current = this;
		NGUIDebug.debugRaycast = debug;
		if (useTouch)
		{
			ProcessTouches();
		}
		else if (useMouse)
		{
			ProcessMouse();
		}
		if (onCustomInput != null)
		{
			onCustomInput();
		}
		if ((useKeyboard || useController) && !disableController && !ignoreControllerInput)
		{
			ProcessOthers();
		}
		if (useMouse && (Object)(object)mHover != (Object)null && currentScheme == ControlScheme.Mouse)
		{
			float num = (string.IsNullOrEmpty(scrollAxisName) ? 0f : GetAxis(scrollAxisName));
			if (num != 0f)
			{
				if (onScroll != null)
				{
					onScroll(mHover, num);
				}
				Notify(mHover, "OnScroll", num);
			}
			if (showTooltips && mTooltipTime != 0f && !UIPopupList.isOpen && (Object)(object)mMouse[0].dragged == (Object)null && (mTooltipTime < RealTime.time || GetKey((KeyCode)304) || GetKey((KeyCode)303)))
			{
				currentTouch = mMouse[0];
				currentTouchID = -1;
				ShowTooltip(mHover);
			}
		}
		if ((Object)(object)mTooltip != (Object)null && !NGUITools.GetActive(mTooltip))
		{
			ShowTooltip(null);
		}
		current = null;
		currentTouchID = -100;
	}

	public void ProcessMouse()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < 3; i++)
		{
			if (Input.GetMouseButtonDown(i))
			{
				currentKey = (KeyCode)(323 + i);
				flag2 = true;
				flag = true;
			}
			else if (Input.GetMouseButton(i))
			{
				currentKey = (KeyCode)(323 + i);
				flag = true;
			}
		}
		if (currentScheme == ControlScheme.Touch)
		{
			return;
		}
		currentTouch = mMouse[0];
		Vector2 val = Vector2.op_Implicit(Input.mousePosition);
		if (currentTouch.ignoreDelta == 0)
		{
			currentTouch.delta = val - currentTouch.pos;
		}
		else
		{
			currentTouch.ignoreDelta--;
			currentTouch.delta.x = 0f;
			currentTouch.delta.y = 0f;
		}
		float sqrMagnitude = ((Vector2)(ref currentTouch.delta)).sqrMagnitude;
		currentTouch.pos = val;
		mLastPos = val;
		bool flag3 = false;
		if (currentScheme != 0)
		{
			if (sqrMagnitude < 0.001f)
			{
				return;
			}
			currentKey = (KeyCode)323;
			flag3 = true;
		}
		else if (sqrMagnitude > 0.001f)
		{
			flag3 = true;
		}
		for (int j = 1; j < 3; j++)
		{
			mMouse[j].pos = currentTouch.pos;
			mMouse[j].delta = currentTouch.delta;
		}
		if (flag || flag3 || mNextRaycast < RealTime.time)
		{
			mNextRaycast = RealTime.time + 0.02f;
			Raycast(currentTouch);
			for (int k = 0; k < 3; k++)
			{
				mMouse[k].current = currentTouch.current;
			}
		}
		bool flag4 = (Object)(object)currentTouch.last != (Object)(object)currentTouch.current;
		bool flag5 = (Object)(object)currentTouch.pressed != (Object)null;
		if (!flag5)
		{
			hoveredObject = currentTouch.current;
		}
		currentTouchID = -1;
		if (flag4)
		{
			currentKey = (KeyCode)323;
		}
		if (!flag && flag3 && (!stickyTooltip || flag4))
		{
			if (mTooltipTime != 0f)
			{
				mTooltipTime = Time.unscaledTime + tooltipDelay;
			}
			else if ((Object)(object)mTooltip != (Object)null)
			{
				ShowTooltip(null);
			}
		}
		if (flag3 && onMouseMove != null)
		{
			onMouseMove(currentTouch.delta);
			currentTouch = null;
		}
		if (flag4 && (flag2 || (flag5 && !flag)))
		{
			hoveredObject = null;
		}
		for (int l = 0; l < 3; l++)
		{
			bool mouseButtonDown = Input.GetMouseButtonDown(l);
			bool mouseButtonUp = Input.GetMouseButtonUp(l);
			if (mouseButtonDown || mouseButtonUp)
			{
				currentKey = (KeyCode)(323 + l);
			}
			currentTouch = mMouse[l];
			currentTouchID = -1 - l;
			currentKey = (KeyCode)(323 + l);
			if (mouseButtonDown)
			{
				currentTouch.pressedCam = currentCamera;
				currentTouch.pressTime = RealTime.time;
			}
			else if ((Object)(object)currentTouch.pressed != (Object)null)
			{
				currentCamera = currentTouch.pressedCam;
			}
			ProcessTouch(mouseButtonDown, mouseButtonUp);
		}
		if (!flag && flag4)
		{
			currentTouch = mMouse[0];
			mTooltipTime = Time.unscaledTime + tooltipDelay;
			currentTouchID = -1;
			currentKey = (KeyCode)323;
			hoveredObject = currentTouch.current;
		}
		currentTouch = null;
		mMouse[0].last = mMouse[0].current;
		for (int m = 1; m < 3; m++)
		{
			mMouse[m].last = mMouse[0].last;
		}
	}

	public void ProcessTouches()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Invalid comparison between Unknown and I4
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Invalid comparison between Unknown and I4
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		int num = ((GetInputTouchCount != null) ? GetInputTouchCount() : Input.touchCount);
		for (int i = 0; i < num; i++)
		{
			TouchPhase phase;
			int fingerId;
			Vector2 position;
			int tapCount;
			if (GetInputTouch == null)
			{
				Touch touch = Input.GetTouch(i);
				phase = ((Touch)(ref touch)).phase;
				fingerId = ((Touch)(ref touch)).fingerId;
				position = ((Touch)(ref touch)).position;
				tapCount = ((Touch)(ref touch)).tapCount;
			}
			else
			{
				Touch touch2 = GetInputTouch(i);
				phase = touch2.phase;
				fingerId = touch2.fingerId;
				position = touch2.position;
				tapCount = touch2.tapCount;
			}
			currentTouchID = ((!allowMultiTouch) ? 1 : fingerId);
			currentTouch = GetTouch(currentTouchID, createIfMissing: true);
			bool flag = (int)phase == 0 || currentTouch.touchBegan;
			bool flag2 = (int)phase == 4 || (int)phase == 3;
			currentTouch.delta = position - currentTouch.pos;
			currentTouch.pos = position;
			currentKey = (KeyCode)0;
			Raycast(currentTouch);
			if (flag)
			{
				currentTouch.pressedCam = currentCamera;
			}
			else if ((Object)(object)currentTouch.pressed != (Object)null)
			{
				currentCamera = currentTouch.pressedCam;
			}
			if (tapCount > 1)
			{
				currentTouch.clickTime = RealTime.time;
			}
			ProcessTouch(flag, flag2);
			if (flag2)
			{
				RemoveTouch(currentTouchID);
			}
			currentTouch.touchBegan = false;
			currentTouch.last = null;
			currentTouch = null;
			if (!allowMultiTouch)
			{
				break;
			}
		}
		if (num == 0)
		{
			if (mUsingTouchEvents)
			{
				mUsingTouchEvents = false;
			}
			else if (useMouse)
			{
				ProcessMouse();
			}
		}
		else
		{
			mUsingTouchEvents = true;
		}
	}

	private void ProcessFakeTouches()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		bool mouseButtonDown = Input.GetMouseButtonDown(0);
		bool mouseButtonUp = Input.GetMouseButtonUp(0);
		bool mouseButton = Input.GetMouseButton(0);
		if (mouseButtonDown || mouseButtonUp || mouseButton)
		{
			currentTouchID = 1;
			currentTouch = mMouse[0];
			currentTouch.touchBegan = mouseButtonDown;
			if (mouseButtonDown)
			{
				currentTouch.pressTime = RealTime.time;
				activeTouches.Add(currentTouch);
			}
			Vector2 val = Vector2.op_Implicit(Input.mousePosition);
			currentTouch.delta = val - currentTouch.pos;
			currentTouch.pos = val;
			Raycast(currentTouch);
			if (mouseButtonDown)
			{
				currentTouch.pressedCam = currentCamera;
			}
			else if ((Object)(object)currentTouch.pressed != (Object)null)
			{
				currentCamera = currentTouch.pressedCam;
			}
			currentKey = (KeyCode)0;
			ProcessTouch(mouseButtonDown, mouseButtonUp);
			if (mouseButtonUp)
			{
				activeTouches.Remove(currentTouch);
			}
			currentTouch.last = null;
			currentTouch = null;
		}
	}

	public void ProcessOthers()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Invalid comparison between Unknown and I4
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Invalid comparison between Unknown and I4
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Invalid comparison between Unknown and I4
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Invalid comparison between Unknown and I4
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Invalid comparison between Unknown and I4
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Invalid comparison between Unknown and I4
		currentTouchID = -100;
		currentTouch = controller;
		bool flag = false;
		bool flag2 = false;
		if ((int)submitKey0 != 0 && GetKeyDown(submitKey0))
		{
			currentKey = submitKey0;
			flag = true;
		}
		else if ((int)submitKey1 != 0 && GetKeyDown(submitKey1))
		{
			currentKey = submitKey1;
			flag = true;
		}
		else if (((int)submitKey0 == 13 || (int)submitKey1 == 13) && GetKeyDown((KeyCode)271))
		{
			currentKey = submitKey0;
			flag = true;
		}
		if ((int)submitKey0 != 0 && GetKeyUp(submitKey0))
		{
			currentKey = submitKey0;
			flag2 = true;
		}
		else if ((int)submitKey1 != 0 && GetKeyUp(submitKey1))
		{
			currentKey = submitKey1;
			flag2 = true;
		}
		else if (((int)submitKey0 == 13 || (int)submitKey1 == 13) && GetKeyUp((KeyCode)271))
		{
			currentKey = submitKey0;
			flag2 = true;
		}
		if (flag)
		{
			currentTouch.pressTime = RealTime.time;
		}
		if ((flag || flag2) && currentScheme == ControlScheme.Controller)
		{
			currentTouch.current = controllerNavigationObject;
			ProcessTouch(flag, flag2);
			currentTouch.last = currentTouch.current;
		}
		KeyCode val = (KeyCode)0;
		if (useController && !ignoreControllerInput)
		{
			if (!disableController && currentScheme == ControlScheme.Controller && ((Object)(object)currentTouch.current == (Object)null || !currentTouch.current.activeInHierarchy))
			{
				currentTouch.current = controllerNavigationObject;
			}
			if (!string.IsNullOrEmpty(verticalAxisName))
			{
				int direction = GetDirection(verticalAxisName);
				if (direction != 0)
				{
					ShowTooltip(null);
					currentScheme = ControlScheme.Controller;
					currentTouch.current = controllerNavigationObject;
					if ((Object)(object)currentTouch.current != (Object)null)
					{
						val = (KeyCode)((direction <= 0) ? 274 : 273);
						if (onNavigate != null)
						{
							onNavigate(currentTouch.current, val);
						}
						Notify(currentTouch.current, "OnNavigate", val);
					}
				}
			}
			if (!string.IsNullOrEmpty(horizontalAxisName))
			{
				int direction2 = GetDirection(horizontalAxisName);
				if (direction2 != 0)
				{
					ShowTooltip(null);
					currentScheme = ControlScheme.Controller;
					currentTouch.current = controllerNavigationObject;
					if ((Object)(object)currentTouch.current != (Object)null)
					{
						val = (KeyCode)((direction2 <= 0) ? 276 : 275);
						if (onNavigate != null)
						{
							onNavigate(currentTouch.current, val);
						}
						Notify(currentTouch.current, "OnNavigate", val);
					}
				}
			}
			float num = (string.IsNullOrEmpty(horizontalPanAxisName) ? 0f : GetAxis(horizontalPanAxisName));
			float num2 = (string.IsNullOrEmpty(verticalPanAxisName) ? 0f : GetAxis(verticalPanAxisName));
			if (num != 0f || num2 != 0f)
			{
				ShowTooltip(null);
				currentScheme = ControlScheme.Controller;
				currentTouch.current = controllerNavigationObject;
				if ((Object)(object)currentTouch.current != (Object)null)
				{
					Vector2 val2 = default(Vector2);
					((Vector2)(ref val2))._002Ector(num, num2);
					val2 *= Time.unscaledDeltaTime;
					if (onPan != null)
					{
						onPan(currentTouch.current, val2);
					}
					Notify(currentTouch.current, "OnPan", val2);
				}
			}
		}
		if ((GetAnyKeyDown == null) ? Input.anyKeyDown : GetAnyKeyDown())
		{
			int i = 0;
			for (int num3 = NGUITools.keys.Length; i < num3; i++)
			{
				KeyCode val3 = NGUITools.keys[i];
				if (val != val3 && GetKeyDown(val3) && (useKeyboard || (int)val3 >= 323) && ((useController && !ignoreControllerInput) || (int)val3 < 330) && (useMouse || ((int)val3 < 323 && (int)val3 > 329)))
				{
					currentKey = val3;
					if (onKey != null)
					{
						onKey(currentTouch.current, val3);
					}
					Notify(currentTouch.current, "OnKey", val3);
				}
			}
		}
		currentTouch = null;
	}

	private void ProcessPress(bool pressed, float click, float drag)
	{
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0589: Unknown result type (might be due to invalid IL or missing references)
		if (pressed)
		{
			if ((Object)(object)mTooltip != (Object)null)
			{
				ShowTooltip(null);
			}
			mTooltipTime = Time.unscaledTime + tooltipDelay;
			currentTouch.pressStarted = true;
			if (onPress != null && Object.op_Implicit((Object)(object)currentTouch.pressed))
			{
				onPress(currentTouch.pressed, state: false);
			}
			Notify(currentTouch.pressed, "OnPress", false);
			if (currentScheme == ControlScheme.Mouse && (Object)(object)hoveredObject == (Object)null && (Object)(object)currentTouch.current != (Object)null)
			{
				hoveredObject = currentTouch.current;
			}
			currentTouch.pressed = currentTouch.current;
			currentTouch.dragged = currentTouch.current;
			currentTouch.clickNotification = ClickNotification.BasedOnDelta;
			currentTouch.totalDelta = Vector2.zero;
			currentTouch.dragStarted = false;
			if (onPress != null && Object.op_Implicit((Object)(object)currentTouch.pressed))
			{
				onPress(currentTouch.pressed, state: true);
			}
			Notify(currentTouch.pressed, "OnPress", true);
			if (!((Object)(object)mSelected != (Object)(object)currentTouch.pressed))
			{
				return;
			}
			mInputFocus = false;
			if (Object.op_Implicit((Object)(object)mSelected))
			{
				Notify(mSelected, "OnSelect", false);
				if (onSelect != null)
				{
					onSelect(mSelected, state: false);
				}
			}
			mSelected = currentTouch.pressed;
			if ((Object)(object)currentTouch.pressed != (Object)null)
			{
				UIKeyNavigation component = currentTouch.pressed.GetComponent<UIKeyNavigation>();
				if ((Object)(object)component != (Object)null)
				{
					controller.current = currentTouch.pressed;
				}
			}
			if (Object.op_Implicit((Object)(object)mSelected))
			{
				mInputFocus = mSelected.activeInHierarchy && (Object)(object)mSelected.GetComponent<UIInput>() != (Object)null;
				if (onSelect != null)
				{
					onSelect(mSelected, state: true);
				}
				Notify(mSelected, "OnSelect", true);
			}
		}
		else
		{
			if (!((Object)(object)currentTouch.pressed != (Object)null) || (((Vector2)(ref currentTouch.delta)).sqrMagnitude == 0f && !((Object)(object)currentTouch.current != (Object)(object)currentTouch.last)))
			{
				return;
			}
			MouseOrTouch mouseOrTouch = currentTouch;
			mouseOrTouch.totalDelta += currentTouch.delta;
			float sqrMagnitude = ((Vector2)(ref currentTouch.totalDelta)).sqrMagnitude;
			bool flag = false;
			if (!currentTouch.dragStarted && (Object)(object)currentTouch.last != (Object)(object)currentTouch.current)
			{
				currentTouch.dragStarted = true;
				currentTouch.delta = currentTouch.totalDelta;
				isDragging = true;
				if (onDragStart != null)
				{
					onDragStart(currentTouch.dragged);
				}
				Notify(currentTouch.dragged, "OnDragStart", null);
				if (onDragOver != null)
				{
					onDragOver(currentTouch.last, currentTouch.dragged);
				}
				Notify(currentTouch.last, "OnDragOver", currentTouch.dragged);
				isDragging = false;
			}
			else if (!currentTouch.dragStarted && drag < sqrMagnitude)
			{
				flag = true;
				currentTouch.dragStarted = true;
				currentTouch.delta = currentTouch.totalDelta;
			}
			if (!currentTouch.dragStarted)
			{
				return;
			}
			if ((Object)(object)mTooltip != (Object)null)
			{
				ShowTooltip(null);
			}
			isDragging = true;
			bool flag2 = currentTouch.clickNotification == ClickNotification.None;
			if (flag)
			{
				if (onDragStart != null)
				{
					onDragStart(currentTouch.dragged);
				}
				Notify(currentTouch.dragged, "OnDragStart", null);
				if (onDragOver != null)
				{
					onDragOver(currentTouch.last, currentTouch.dragged);
				}
				Notify(currentTouch.current, "OnDragOver", currentTouch.dragged);
			}
			else if ((Object)(object)currentTouch.last != (Object)(object)currentTouch.current)
			{
				if (onDragOut != null)
				{
					onDragOut(currentTouch.last, currentTouch.dragged);
				}
				Notify(currentTouch.last, "OnDragOut", currentTouch.dragged);
				if (onDragOver != null)
				{
					onDragOver(currentTouch.last, currentTouch.dragged);
				}
				Notify(currentTouch.current, "OnDragOver", currentTouch.dragged);
			}
			if (onDrag != null)
			{
				onDrag(currentTouch.dragged, currentTouch.delta);
			}
			Notify(currentTouch.dragged, "OnDrag", currentTouch.delta);
			currentTouch.last = currentTouch.current;
			isDragging = false;
			if (flag2)
			{
				currentTouch.clickNotification = ClickNotification.None;
			}
			else if (currentTouch.clickNotification == ClickNotification.BasedOnDelta && click < sqrMagnitude)
			{
				currentTouch.clickNotification = ClickNotification.None;
			}
		}
	}

	private void ProcessRelease(bool isMouse, float drag)
	{
		if (currentTouch == null)
		{
			return;
		}
		currentTouch.pressStarted = false;
		if ((Object)(object)currentTouch.pressed != (Object)null)
		{
			if (currentTouch.dragStarted)
			{
				if (onDragOut != null)
				{
					onDragOut(currentTouch.last, currentTouch.dragged);
				}
				Notify(currentTouch.last, "OnDragOut", currentTouch.dragged);
				if (onDragEnd != null)
				{
					onDragEnd(currentTouch.dragged);
				}
				Notify(currentTouch.dragged, "OnDragEnd", null);
			}
			if (onPress != null)
			{
				onPress(currentTouch.pressed, state: false);
			}
			Notify(currentTouch.pressed, "OnPress", false);
			if (isMouse && HasCollider(currentTouch.pressed))
			{
				if ((Object)(object)mHover == (Object)(object)currentTouch.current)
				{
					if (onHover != null)
					{
						onHover(currentTouch.current, state: true);
					}
					Notify(currentTouch.current, "OnHover", true);
				}
				else
				{
					hoveredObject = currentTouch.current;
				}
			}
			if ((Object)(object)currentTouch.dragged == (Object)(object)currentTouch.current || (currentScheme != ControlScheme.Controller && currentTouch.clickNotification != 0 && ((Vector2)(ref currentTouch.totalDelta)).sqrMagnitude < drag))
			{
				if (currentTouch.clickNotification != 0 && (Object)(object)currentTouch.pressed == (Object)(object)currentTouch.current)
				{
					ShowTooltip(null);
					float time = RealTime.time;
					if (onClick != null)
					{
						onClick(currentTouch.pressed);
					}
					Notify(currentTouch.pressed, "OnClick", null);
					if (currentTouch.clickTime + 0.35f > time)
					{
						if (onDoubleClick != null)
						{
							onDoubleClick(currentTouch.pressed);
						}
						Notify(currentTouch.pressed, "OnDoubleClick", null);
					}
					currentTouch.clickTime = time;
				}
			}
			else if (currentTouch.dragStarted)
			{
				if (onDrop != null)
				{
					onDrop(currentTouch.current, currentTouch.dragged);
				}
				Notify(currentTouch.current, "OnDrop", currentTouch.dragged);
			}
		}
		currentTouch.dragStarted = false;
		currentTouch.pressed = null;
		currentTouch.dragged = null;
	}

	private bool HasCollider(GameObject go)
	{
		if ((Object)(object)go == (Object)null)
		{
			return false;
		}
		Collider component = go.GetComponent<Collider>();
		if ((Object)(object)component != (Object)null)
		{
			return component.enabled;
		}
		Collider2D component2 = go.GetComponent<Collider2D>();
		return (Object)(object)component2 != (Object)null && ((Behaviour)component2).enabled;
	}

	public void ProcessTouch(bool pressed, bool released)
	{
		if (released)
		{
			mTooltipTime = 0f;
		}
		bool flag = currentScheme == ControlScheme.Mouse;
		float num = ((!flag) ? touchDragThreshold : mouseDragThreshold);
		float num2 = ((!flag) ? touchClickThreshold : mouseClickThreshold);
		num *= num;
		num2 *= num2;
		if ((Object)(object)currentTouch.pressed != (Object)null)
		{
			if (released)
			{
				ProcessRelease(flag, num);
			}
			ProcessPress(pressed, num2, num);
			if (currentTouch.deltaTime > tooltipDelay && (Object)(object)currentTouch.pressed == (Object)(object)currentTouch.current && mTooltipTime != 0f && !currentTouch.dragStarted)
			{
				mTooltipTime = 0f;
				currentTouch.clickNotification = ClickNotification.None;
				if (longPressTooltip)
				{
					ShowTooltip(currentTouch.pressed);
				}
				if (onLongPress != null)
				{
					onLongPress(currentTouch.current);
				}
				Notify(currentTouch.current, "OnLongPress", null);
			}
		}
		else if (flag || pressed || released)
		{
			ProcessPress(pressed, num2, num);
			if (released)
			{
				ProcessRelease(flag, num);
			}
		}
	}

	public static void CancelNextTooltip()
	{
		mTooltipTime = 0f;
	}

	public static bool ShowTooltip(GameObject go)
	{
		if ((Object)(object)mTooltip != (Object)(object)go)
		{
			if ((Object)(object)mTooltip != (Object)null)
			{
				if (onTooltip != null)
				{
					onTooltip(mTooltip, state: false);
				}
				Notify(mTooltip, "OnTooltip", false);
			}
			mTooltip = go;
			mTooltipTime = 0f;
			if ((Object)(object)mTooltip != (Object)null)
			{
				if (onTooltip != null)
				{
					onTooltip(mTooltip, state: true);
				}
				Notify(mTooltip, "OnTooltip", true);
			}
			return true;
		}
		return false;
	}

	public static bool HideTooltip()
	{
		return ShowTooltip(null);
	}
}
