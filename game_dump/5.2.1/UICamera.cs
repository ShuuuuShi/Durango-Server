using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Event System (UICamera)")]
[RequireComponent(typeof(Camera))]
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

		public bool isOverUI
		{
			get
			{
				if (current != null && current != fallThrough)
				{
					return NGUITools.FindInParents<UIRoot>(current) != null;
				}
				return false;
			}
		}
	}

	public enum EventType
	{
		World_3D,
		UI_3D,
		World_2D,
		UI_2D
	}

	public delegate bool GetKeyStateFunc(KeyCode key);

	public delegate float GetAxisFunc(string name);

	public delegate bool GetAnyKeyFunc();

	public delegate void OnScreenResize();

	public enum ProcessEventsIn
	{
		Update,
		LateUpdate
	}

	public delegate void OnCustomInput();

	public delegate void OnSchemeChange();

	public delegate void MoveDelegate(Vector2 delta);

	public delegate void VoidDelegate(GameObject go);

	public delegate void BoolDelegate(GameObject go, bool state);

	public delegate void FloatDelegate(GameObject go, float delta);

	public delegate void VectorDelegate(GameObject go, Vector2 delta);

	public delegate void ObjectDelegate(GameObject go, GameObject obj);

	public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

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

	public delegate int GetTouchCountCallback();

	public delegate Touch GetTouchCallback(int index);

	public static BetterList<UICamera> list = new BetterList<UICamera>();

	public static GetKeyStateFunc GetKeyDown = (KeyCode key) => (key < KeyCode.JoystickButton0 || !ignoreControllerInput) && Input.GetKeyDown(key);

	public static GetKeyStateFunc GetKeyUp = (KeyCode key) => (key < KeyCode.JoystickButton0 || !ignoreControllerInput) && Input.GetKeyUp(key);

	public static GetKeyStateFunc GetKey = (KeyCode key) => (key < KeyCode.JoystickButton0 || !ignoreControllerInput) && Input.GetKey(key);

	public static GetAxisFunc GetAxis = (string axis) => (!ignoreControllerInput) ? Input.GetAxis(axis) : 0f;

	public static GetAnyKeyFunc GetAnyKeyDown;

	public static OnScreenResize onScreenResize;

	public EventType eventType = EventType.UI_3D;

	public bool eventsGoToColliders;

	public LayerMask eventReceiverMask = -1;

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

	public KeyCode submitKey0 = KeyCode.Return;

	public KeyCode submitKey1 = KeyCode.JoystickButton0;

	public KeyCode cancelKey0 = KeyCode.Escape;

	public KeyCode cancelKey1 = KeyCode.JoystickButton1;

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

	private static KeyCode mCurrentKey = KeyCode.Alpha0;

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

	public static bool useHoverEvent = true;

	private static GameObject mRayHitObject;

	private static GameObject mHover;

	private static GameObject mSelected;

	private static DepthEntry mHit = default(DepthEntry);

	private static BetterList<DepthEntry> mHits = new BetterList<DepthEntry>();

	private static RaycastHit[] _raycastHits = new RaycastHit[16];

	private static int _prevHitCount;

	private static Plane m2DPlane = new Plane(Vector3.back, 0f);

	private static float mNextEvent = 0f;

	private static int mNotifying = 0;

	private static bool disableControllerCheck = true;

	private static bool mUsingTouchEvents = true;

	public static GetTouchCountCallback GetInputTouchCount;

	public static GetTouchCallback GetInputTouch;

	[Obsolete("Use new OnDragStart / OnDragOver / OnDragOut / OnDragEnd events instead")]
	public bool stickyPress => true;

	public static bool disableController
	{
		get
		{
			if (mDisableController)
			{
				return !UIPopupList.isOpen;
			}
			return false;
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
			return mLastPos;
		}
		set
		{
			mLastPos = value;
		}
	}

	public static Vector2 lastEventPosition
	{
		get
		{
			if (currentScheme == ControlScheme.Controller)
			{
				GameObject gameObject = hoveredObject;
				if (gameObject != null)
				{
					Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(gameObject.transform);
					return NGUITools.FindCameraForLayer(gameObject.layer).WorldToScreenPoint(bounds.center);
				}
			}
			return mLastPos;
		}
		set
		{
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
			if (mCurrentKey == KeyCode.None)
			{
				return ControlScheme.Touch;
			}
			if (mCurrentKey >= KeyCode.JoystickButton0)
			{
				return ControlScheme.Controller;
			}
			if (current != null && mLastScheme == ControlScheme.Controller && (mCurrentKey == current.submitKey0 || mCurrentKey == current.submitKey1))
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
				currentKey = KeyCode.Mouse0;
				break;
			case ControlScheme.Controller:
				currentKey = KeyCode.JoystickButton0;
				break;
			case ControlScheme.Touch:
				currentKey = KeyCode.None;
				break;
			default:
				currentKey = KeyCode.Alpha0;
				break;
			}
			mLastScheme = value;
		}
	}

	public static KeyCode currentKey
	{
		get
		{
			return mCurrentKey;
		}
		set
		{
			if (mCurrentKey == value)
			{
				return;
			}
			ControlScheme num = mLastScheme;
			mCurrentKey = value;
			mLastScheme = currentScheme;
			if (num != mLastScheme)
			{
				HideTooltip();
				if (mLastScheme == ControlScheme.Mouse)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
				else if (current != null && current.autoHideCursor)
				{
					Cursor.visible = false;
					Cursor.lockState = CursorLockMode.Locked;
					mMouse[0].ignoreDelta = 2;
				}
				if (onSchemeChange != null)
				{
					onSchemeChange();
				}
			}
		}
	}

	public static Ray currentRay
	{
		get
		{
			if (currentCamera != null && currentTouch != null)
			{
				return currentCamera.ScreenPointToRay(currentTouch.pos);
			}
			return default(Ray);
		}
	}

	public static bool inputHasFocus
	{
		get
		{
			if (mInputFocus && (bool)mSelected && mSelected.activeInHierarchy)
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

	private bool handlesEvents => eventHandler == this;

	public Camera cachedCamera
	{
		get
		{
			if (mCam == null)
			{
				mCam = GetComponent<Camera>();
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
				if (mouseOrTouch.pressed != null && mouseOrTouch.pressed != fallThrough && NGUITools.FindInParents<UIRoot>(mouseOrTouch.pressed) != null)
				{
					return true;
				}
			}
			if (mMouse[0].current != null && mMouse[0].current != fallThrough && NGUITools.FindInParents<UIRoot>(mMouse[0].current) != null)
			{
				return true;
			}
			if (controller.pressed != null && controller.pressed != fallThrough && NGUITools.FindInParents<UIRoot>(controller.pressed) != null)
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
			if ((bool)mHover && mHover.activeInHierarchy)
			{
				return mHover;
			}
			mHover = null;
			return null;
		}
		set
		{
			if (mHover == value)
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
			if ((bool)mSelected && currentScheme == ControlScheme.Controller)
			{
				Notify(mSelected, "OnSelect", false);
				if (onSelect != null)
				{
					onSelect(mSelected, state: false);
				}
				mSelected = null;
			}
			if ((bool)mHover && useHoverEvent)
			{
				Notify(mHover, "OnHover", false);
				if (onHover != null)
				{
					onHover(mHover, state: false);
				}
			}
			mHover = value;
			currentTouch.clickNotification = ClickNotification.None;
			if ((bool)mHover)
			{
				if (mHover != controller.current && mHover.GetComponent<UIKeyNavigation>() != null)
				{
					controller.current = mHover;
				}
				if (flag)
				{
					UICamera uICamera2 = ((!(mHover != null)) ? list[0] : FindCameraForLayer(mHover.layer));
					if (uICamera2 != null)
					{
						current = uICamera2;
						currentCamera = uICamera2.cachedCamera;
					}
				}
				if (useHoverEvent)
				{
					if (onHover != null)
					{
						onHover(mHover, state: true);
					}
					Notify(mHover, "OnHover", true);
				}
			}
			if (flag)
			{
				current = uICamera;
				currentCamera = ((!(uICamera != null)) ? null : uICamera.cachedCamera);
				currentTouch = null;
				currentTouchID = -100;
			}
		}
	}

	public static GameObject controllerNavigationObject
	{
		get
		{
			if ((bool)controller.current && controller.current.activeInHierarchy)
			{
				return controller.current;
			}
			if (currentScheme == ControlScheme.Controller && current != null && current.useController && !ignoreControllerInput && UIKeyNavigation.list.size > 0)
			{
				for (int i = 0; i < UIKeyNavigation.list.size; i++)
				{
					UIKeyNavigation uIKeyNavigation = UIKeyNavigation.list[i];
					if ((bool)uIKeyNavigation && uIKeyNavigation.constraint != UIKeyNavigation.Constraint.Explicit && uIKeyNavigation.startsSelected)
					{
						hoveredObject = uIKeyNavigation.gameObject;
						controller.current = mHover;
						return mHover;
					}
				}
				if (mHover == null)
				{
					for (int j = 0; j < UIKeyNavigation.list.size; j++)
					{
						UIKeyNavigation uIKeyNavigation2 = UIKeyNavigation.list[j];
						if ((bool)uIKeyNavigation2 && uIKeyNavigation2.constraint != UIKeyNavigation.Constraint.Explicit)
						{
							hoveredObject = uIKeyNavigation2.gameObject;
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
			if (controller.current != value && (bool)controller.current)
			{
				if (useHoverEvent)
				{
					Notify(controller.current, "OnHover", false);
					if (onHover != null)
					{
						onHover(controller.current, state: false);
					}
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
			if ((bool)mSelected && mSelected.activeInHierarchy)
			{
				return mSelected;
			}
			mSelected = null;
			return null;
		}
		set
		{
			if (mSelected == value)
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
			if ((bool)mSelected)
			{
				Notify(mSelected, "OnSelect", false);
				if (onSelect != null)
				{
					onSelect(mSelected, state: false);
				}
			}
			mSelected = value;
			currentTouch.clickNotification = ClickNotification.None;
			if (value != null && value.GetComponent<UIKeyNavigation>() != null)
			{
				controller.current = value;
			}
			if ((bool)mSelected && flag)
			{
				UICamera uICamera2 = ((!(mSelected != null)) ? list[0] : FindCameraForLayer(mSelected.layer));
				if (uICamera2 != null)
				{
					current = uICamera2;
					currentCamera = uICamera2.cachedCamera;
				}
			}
			if ((bool)mSelected)
			{
				mInputFocus = mSelected.activeInHierarchy && mSelected.GetComponent<UIInput>() != null;
				if (onSelect != null)
				{
					onSelect(mSelected, state: true);
				}
				Notify(mSelected, "OnSelect", true);
			}
			if (flag)
			{
				current = uICamera;
				currentCamera = ((!(uICamera != null)) ? null : uICamera.cachedCamera);
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
				if (activeTouches[i].dragged != null)
				{
					num++;
				}
			}
			for (int j = 0; j < mMouse.Length; j++)
			{
				if (mMouse[j].dragged != null)
				{
					num++;
				}
			}
			if (controller.dragged != null)
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
			if (uICamera != null)
			{
				return uICamera.cachedCamera;
			}
			return null;
		}
	}

	public static UICamera eventHandler
	{
		get
		{
			for (int i = 0; i < list.size; i++)
			{
				UICamera uICamera = list.buffer[i];
				if (!(uICamera == null) && uICamera.enabled && NGUITools.GetActive(uICamera.gameObject))
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
			if (mMouse[i].pressed == go)
			{
				return true;
			}
		}
		int j = 0;
		for (int count = activeTouches.Count; j < count; j++)
		{
			if (activeTouches[j].pressed == go)
			{
				return true;
			}
		}
		if (controller.pressed == go)
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
			if (activeTouches[i].pressed != null)
			{
				num++;
			}
		}
		for (int j = 0; j < mMouse.Length; j++)
		{
			if (mMouse[j].pressed != null)
			{
				num++;
			}
		}
		if (controller.pressed != null)
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
		while (trans != null)
		{
			if (trans.GetComponent<UIPanel>() != null)
			{
				return null;
			}
			Rigidbody component = trans.GetComponent<Rigidbody>();
			if (component != null)
			{
				return component;
			}
			trans = trans.parent;
		}
		return null;
	}

	private static Rigidbody2D FindRootRigidbody2D(Transform trans)
	{
		while (trans != null)
		{
			if (trans.GetComponent<UIPanel>() != null)
			{
				return null;
			}
			Rigidbody2D component = trans.GetComponent<Rigidbody2D>();
			if (component != null)
			{
				return component;
			}
			trans = trans.parent;
		}
		return null;
	}

	public static void Raycast(MouseOrTouch touch)
	{
		if (!Raycast(touch.pos))
		{
			mRayHitObject = fallThrough;
		}
		if (mRayHitObject == null)
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

	private static RaycastHit[] RayCast(Ray ray, float dist, int mask, out int count)
	{
		if (_prevHitCount >= _raycastHits.Length)
		{
			_raycastHits = new RaycastHit[_prevHitCount * 2];
		}
		count = Physics.RaycastNonAlloc(ray, _raycastHits, dist, mask);
		_prevHitCount = count;
		return _raycastHits;
	}

	public static bool Raycast(Vector3 inPos)
	{
		for (int i = 0; i < list.size; i++)
		{
			UICamera uICamera = list.buffer[i];
			if (!uICamera.enabled || !NGUITools.GetActive(uICamera.gameObject))
			{
				continue;
			}
			currentCamera = uICamera.cachedCamera;
			Vector3 vector = currentCamera.ScreenToViewportPoint(inPos);
			if (float.IsNaN(vector.x) || float.IsNaN(vector.y) || vector.x < -10f || vector.x > 10f || vector.y < -10f || vector.y > 10f)
			{
				continue;
			}
			Ray ray = currentCamera.ScreenPointToRay(inPos);
			int num = currentCamera.cullingMask & (int)uICamera.eventReceiverMask;
			float enter = ((!(uICamera.rangeDistance > 0f)) ? (currentCamera.farClipPlane - currentCamera.nearClipPlane) : uICamera.rangeDistance);
			if (uICamera.eventType == EventType.World_3D)
			{
				if (!Physics.Raycast(ray, out lastHit, enter, num))
				{
					continue;
				}
				lastWorldPosition = lastHit.point;
				mRayHitObject = lastHit.collider.gameObject;
				if (!uICamera.eventsGoToColliders)
				{
					Rigidbody rigidbody = FindRootRigidbody(mRayHitObject.transform);
					if (rigidbody != null)
					{
						mRayHitObject = rigidbody.gameObject;
					}
				}
				return true;
			}
			if (uICamera.eventType == EventType.UI_3D)
			{
				int count;
				RaycastHit[] array = RayCast(ray, enter, num, out count);
				if (count > 1)
				{
					for (int j = 0; j < count; j++)
					{
						GameObject gameObject = array[j].collider.gameObject;
						UIWidget component = gameObject.GetComponent<UIWidget>();
						if (component != null)
						{
							if (!component.isVisible || (component.hitCheck != null && !component.hitCheck(array[j].point)))
							{
								continue;
							}
						}
						else
						{
							UIRect uIRect = NGUITools.FindInParents<UIRect>(gameObject);
							if (uIRect != null && uIRect.finalAlpha < 0.001f)
							{
								continue;
							}
						}
						mHit.depth = NGUITools.CalculateRaycastDepth(gameObject);
						if (mHit.depth != int.MaxValue)
						{
							mHit.hit = array[j];
							mHit.point = array[j].point;
							mHit.go = array[j].collider.gameObject;
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
					if (count != 1)
					{
						continue;
					}
					GameObject gameObject2 = array[0].collider.gameObject;
					UIWidget component2 = gameObject2.GetComponent<UIWidget>();
					if (component2 != null)
					{
						if (!component2.isVisible || (component2.hitCheck != null && !component2.hitCheck(array[0].point)))
						{
							continue;
						}
					}
					else
					{
						UIRect uIRect2 = NGUITools.FindInParents<UIRect>(gameObject2);
						if (uIRect2 != null && uIRect2.finalAlpha < 0.001f)
						{
							continue;
						}
					}
					if (IsVisible(array[0].point, array[0].collider.gameObject))
					{
						lastHit = array[0];
						lastWorldPosition = array[0].point;
						mRayHitObject = lastHit.collider.gameObject;
						return true;
					}
				}
			}
			else
			{
				if (uICamera.eventType == EventType.World_2D)
				{
					if (!m2DPlane.Raycast(ray, out enter))
					{
						continue;
					}
					Vector3 point = ray.GetPoint(enter);
					Collider2D collider2D = Physics2D.OverlapPoint(point, num);
					if (!collider2D)
					{
						continue;
					}
					lastWorldPosition = point;
					mRayHitObject = collider2D.gameObject;
					if (!uICamera.eventsGoToColliders)
					{
						Rigidbody2D rigidbody2D = FindRootRigidbody2D(mRayHitObject.transform);
						if (rigidbody2D != null)
						{
							mRayHitObject = rigidbody2D.gameObject;
						}
					}
					return true;
				}
				if (uICamera.eventType != EventType.UI_2D || !m2DPlane.Raycast(ray, out enter))
				{
					continue;
				}
				lastWorldPosition = ray.GetPoint(enter);
				Collider2D[] array2 = Physics2D.OverlapPointAll(lastWorldPosition, num);
				if (array2.Length > 1)
				{
					for (int l = 0; l < array2.Length; l++)
					{
						GameObject gameObject3 = array2[l].gameObject;
						UIWidget component3 = gameObject3.GetComponent<UIWidget>();
						if (component3 != null)
						{
							if (!component3.isVisible || (component3.hitCheck != null && !component3.hitCheck(lastWorldPosition)))
							{
								continue;
							}
						}
						else
						{
							UIRect uIRect3 = NGUITools.FindInParents<UIRect>(gameObject3);
							if (uIRect3 != null && uIRect3.finalAlpha < 0.001f)
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
					GameObject gameObject4 = array2[0].gameObject;
					UIWidget component4 = gameObject4.GetComponent<UIWidget>();
					if (component4 != null)
					{
						if (!component4.isVisible || (component4.hitCheck != null && !component4.hitCheck(lastWorldPosition)))
						{
							continue;
						}
					}
					else
					{
						UIRect uIRect4 = NGUITools.FindInParents<UIRect>(gameObject4);
						if (uIRect4 != null && uIRect4.finalAlpha < 0.001f)
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
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(go);
		while (uIPanel != null)
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
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(de.go);
		while (uIPanel != null)
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
		return hoveredObject == go;
	}

	public static UICamera FindCameraForLayer(int layer)
	{
		int num = 1 << layer;
		for (int i = 0; i < list.size; i++)
		{
			UICamera uICamera = list.buffer[i];
			Camera camera = uICamera.cachedCamera;
			if (camera != null && (camera.cullingMask & num) != 0)
			{
				return uICamera;
			}
		}
		return null;
	}

	private static int GetDirection(KeyCode up, KeyCode down)
	{
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
				currentKey = KeyCode.JoystickButton0;
				mNextEvent = time + 0.25f;
				return 1;
			}
			if (num < -0.75f)
			{
				currentKey = KeyCode.JoystickButton0;
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
		if (currentScheme == ControlScheme.Controller && UIPopupList.isOpen && UIPopupList.current.source == go && UIPopupList.isOpen)
		{
			go = UIPopupList.current.gameObject;
		}
		if ((bool)go && go.activeInHierarchy)
		{
			mNotifying++;
			go.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
			if (mGenericHandler != null && mGenericHandler != go)
			{
				mGenericHandler.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
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
		mWidth = Screen.width;
		mHeight = Screen.height;
		mMouse[0].pos = Input.mousePosition;
		for (int i = 1; i < 3; i++)
		{
			mMouse[i].pos = mMouse[0].pos;
			mMouse[i].lastPos = mMouse[0].pos;
		}
		mLastPos = mMouse[0].pos;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs == null)
		{
			return;
		}
		for (int j = 0; j < commandLineArgs.Length; j++)
		{
			switch (commandLineArgs[j])
			{
			case "-noMouse":
				useMouse = false;
				break;
			case "-noTouch":
				useTouch = false;
				break;
			case "-noController":
				useController = false;
				ignoreControllerInput = true;
				break;
			case "-noJoystick":
				useController = false;
				ignoreControllerInput = true;
				break;
			case "-useMouse":
				useMouse = true;
				break;
			case "-useTouch":
				useTouch = true;
				break;
			case "-useController":
				useController = true;
				break;
			case "-useJoystick":
				useController = true;
				break;
			}
		}
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
		if (eventType != 0 && cachedCamera.transparencySortMode != TransparencySortMode.Orthographic)
		{
			cachedCamera.transparencySortMode = TransparencySortMode.Orthographic;
		}
		if (!Application.isPlaying)
		{
			return;
		}
		if (fallThrough == null)
		{
			UIRoot uIRoot = NGUITools.FindInParents<UIRoot>(base.gameObject);
			fallThrough = ((!(uIRoot != null)) ? base.gameObject : uIRoot.gameObject);
		}
		cachedCamera.eventMask = 0;
		if (!ignoreControllerInput && disableControllerCheck && useController && handlesEvents)
		{
			disableControllerCheck = false;
			if (!string.IsNullOrEmpty(horizontalAxisName) && Mathf.Abs(GetAxis(horizontalAxisName)) > 0.1f)
			{
				ignoreControllerInput = true;
			}
			else if (!string.IsNullOrEmpty(verticalAxisName) && Mathf.Abs(GetAxis(verticalAxisName)) > 0.1f)
			{
				ignoreControllerInput = true;
			}
			else if (!string.IsNullOrEmpty(horizontalPanAxisName) && Mathf.Abs(GetAxis(horizontalPanAxisName)) > 0.1f)
			{
				ignoreControllerInput = true;
			}
			else if (!string.IsNullOrEmpty(verticalPanAxisName) && Mathf.Abs(GetAxis(verticalPanAxisName)) > 0.1f)
			{
				ignoreControllerInput = true;
			}
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
		if (useMouse && mHover != null && currentScheme == ControlScheme.Mouse)
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
			if (showTooltips && mTooltipTime != 0f && !UIPopupList.isOpen && mMouse[0].dragged == null && (mTooltipTime < RealTime.time || GetKey(KeyCode.LeftShift) || GetKey(KeyCode.RightShift)))
			{
				currentTouch = mMouse[0];
				currentTouchID = -1;
				ShowTooltip(mHover);
			}
		}
		if (mTooltip != null && !NGUITools.GetActive(mTooltip))
		{
			ShowTooltip(null);
		}
		current = null;
		currentTouchID = -100;
	}

	public void ProcessMouse()
	{
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
		Vector2 vector = Input.mousePosition;
		if (currentTouch.ignoreDelta == 0)
		{
			currentTouch.delta = vector - currentTouch.pos;
		}
		else
		{
			currentTouch.ignoreDelta--;
			currentTouch.delta.x = 0f;
			currentTouch.delta.y = 0f;
		}
		float sqrMagnitude = currentTouch.delta.sqrMagnitude;
		currentTouch.pos = vector;
		mLastPos = vector;
		bool flag3 = false;
		if (currentScheme != 0)
		{
			if (sqrMagnitude < 0.001f)
			{
				return;
			}
			currentKey = KeyCode.Mouse0;
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
		bool flag4 = currentTouch.last != currentTouch.current;
		bool flag5 = currentTouch.pressed != null;
		if (!flag5)
		{
			hoveredObject = currentTouch.current;
		}
		currentTouchID = -1;
		if (flag4)
		{
			currentKey = KeyCode.Mouse0;
		}
		if (!flag && flag3 && (!stickyTooltip || flag4))
		{
			if (mTooltipTime != 0f)
			{
				mTooltipTime = Time.unscaledTime + tooltipDelay;
			}
			else if (mTooltip != null)
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
			else if (currentTouch.pressed != null)
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
			currentKey = KeyCode.Mouse0;
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
		int num = ((GetInputTouchCount != null) ? GetInputTouchCount() : Input.touchCount);
		for (int i = 0; i < num; i++)
		{
			TouchPhase phase;
			int fingerId;
			Vector2 position;
			int tapCount;
			if (GetInputTouch == null)
			{
				UnityEngine.Touch touch = Input.GetTouch(i);
				phase = touch.phase;
				fingerId = touch.fingerId;
				position = touch.position;
				tapCount = touch.tapCount;
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
			bool flag = phase == TouchPhase.Began || currentTouch.touchBegan;
			bool flag2 = phase == TouchPhase.Canceled || phase == TouchPhase.Ended;
			currentTouch.delta = position - currentTouch.pos;
			currentTouch.pos = position;
			currentKey = KeyCode.None;
			Raycast(currentTouch);
			if (flag)
			{
				currentTouch.pressedCam = currentCamera;
			}
			else if (currentTouch.pressed != null)
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
			Vector2 vector = Input.mousePosition;
			currentTouch.delta = vector - currentTouch.pos;
			currentTouch.pos = vector;
			Raycast(currentTouch);
			if (mouseButtonDown)
			{
				currentTouch.pressedCam = currentCamera;
			}
			else if (currentTouch.pressed != null)
			{
				currentCamera = currentTouch.pressedCam;
			}
			currentKey = KeyCode.None;
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
		currentTouchID = -100;
		currentTouch = controller;
		bool flag = false;
		bool flag2 = false;
		if (submitKey0 != 0 && GetKeyDown(submitKey0))
		{
			currentKey = submitKey0;
			flag = true;
		}
		else if (submitKey1 != 0 && GetKeyDown(submitKey1))
		{
			currentKey = submitKey1;
			flag = true;
		}
		else if ((submitKey0 == KeyCode.Return || submitKey1 == KeyCode.Return) && GetKeyDown(KeyCode.KeypadEnter))
		{
			currentKey = submitKey0;
			flag = true;
		}
		if (submitKey0 != 0 && GetKeyUp(submitKey0))
		{
			currentKey = submitKey0;
			flag2 = true;
		}
		else if (submitKey1 != 0 && GetKeyUp(submitKey1))
		{
			currentKey = submitKey1;
			flag2 = true;
		}
		else if ((submitKey0 == KeyCode.Return || submitKey1 == KeyCode.Return) && GetKeyUp(KeyCode.KeypadEnter))
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
		KeyCode keyCode = KeyCode.None;
		if (useController && !ignoreControllerInput)
		{
			if (!disableController && currentScheme == ControlScheme.Controller && (currentTouch.current == null || !currentTouch.current.activeInHierarchy))
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
					if (currentTouch.current != null)
					{
						keyCode = ((direction <= 0) ? KeyCode.DownArrow : KeyCode.UpArrow);
						if (onNavigate != null)
						{
							onNavigate(currentTouch.current, keyCode);
						}
						Notify(currentTouch.current, "OnNavigate", keyCode);
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
					if (currentTouch.current != null)
					{
						keyCode = ((direction2 <= 0) ? KeyCode.LeftArrow : KeyCode.RightArrow);
						if (onNavigate != null)
						{
							onNavigate(currentTouch.current, keyCode);
						}
						Notify(currentTouch.current, "OnNavigate", keyCode);
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
				if (currentTouch.current != null)
				{
					Vector2 vector = new Vector2(num, num2);
					vector *= Time.unscaledDeltaTime;
					if (onPan != null)
					{
						onPan(currentTouch.current, vector);
					}
					Notify(currentTouch.current, "OnPan", vector);
				}
			}
		}
		if ((GetAnyKeyDown == null) ? Input.anyKeyDown : GetAnyKeyDown())
		{
			int i = 0;
			for (int num3 = NGUITools.keys.Length; i < num3; i++)
			{
				KeyCode keyCode2 = NGUITools.keys[i];
				if (keyCode != keyCode2 && GetKeyDown(keyCode2) && (useKeyboard || keyCode2 >= KeyCode.Mouse0) && ((useController && !ignoreControllerInput) || keyCode2 < KeyCode.JoystickButton0) && (useMouse || (keyCode2 < KeyCode.Mouse0 && keyCode2 > KeyCode.Mouse6)))
				{
					currentKey = keyCode2;
					if (onKey != null)
					{
						onKey(currentTouch.current, keyCode2);
					}
					Notify(currentTouch.current, "OnKey", keyCode2);
				}
			}
		}
		currentTouch = null;
	}

	private void ProcessPress(bool pressed, float click, float drag)
	{
		if (pressed)
		{
			if (mTooltip != null)
			{
				ShowTooltip(null);
			}
			mTooltipTime = Time.unscaledTime + tooltipDelay;
			currentTouch.pressStarted = true;
			if (onPress != null && (bool)currentTouch.pressed)
			{
				onPress(currentTouch.pressed, state: false);
			}
			Notify(currentTouch.pressed, "OnPress", false);
			if (currentScheme == ControlScheme.Mouse && hoveredObject == null && currentTouch.current != null)
			{
				hoveredObject = currentTouch.current;
			}
			currentTouch.pressed = currentTouch.current;
			currentTouch.dragged = currentTouch.current;
			currentTouch.clickNotification = ClickNotification.BasedOnDelta;
			currentTouch.totalDelta = Vector2.zero;
			currentTouch.dragStarted = false;
			if (onPress != null && (bool)currentTouch.pressed)
			{
				onPress(currentTouch.pressed, state: true);
			}
			Notify(currentTouch.pressed, "OnPress", true);
			if (!(mSelected != currentTouch.pressed))
			{
				return;
			}
			mInputFocus = false;
			if ((bool)mSelected)
			{
				Notify(mSelected, "OnSelect", false);
				if (onSelect != null)
				{
					onSelect(mSelected, state: false);
				}
			}
			mSelected = currentTouch.pressed;
			if (currentTouch.pressed != null && currentTouch.pressed.GetComponent<UIKeyNavigation>() != null)
			{
				controller.current = currentTouch.pressed;
			}
			if ((bool)mSelected)
			{
				mInputFocus = mSelected.activeInHierarchy && mSelected.GetComponent<UIInput>() != null;
				if (onSelect != null)
				{
					onSelect(mSelected, state: true);
				}
				Notify(mSelected, "OnSelect", true);
			}
		}
		else
		{
			if (!(currentTouch.pressed != null) || (currentTouch.delta.sqrMagnitude == 0f && !(currentTouch.current != currentTouch.last)))
			{
				return;
			}
			currentTouch.totalDelta += currentTouch.delta;
			float sqrMagnitude = currentTouch.totalDelta.sqrMagnitude;
			bool flag = false;
			if (!currentTouch.dragStarted && currentTouch.last != currentTouch.current)
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
			if (mTooltip != null)
			{
				ShowTooltip(null);
			}
			isDragging = true;
			bool num = currentTouch.clickNotification == ClickNotification.None;
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
			else if (currentTouch.last != currentTouch.current)
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
			if (num)
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
		if (currentTouch.pressed != null)
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
				if (mHover == currentTouch.current)
				{
					if (useHoverEvent)
					{
						if (onHover != null)
						{
							onHover(currentTouch.current, state: true);
						}
						Notify(currentTouch.current, "OnHover", true);
					}
				}
				else
				{
					hoveredObject = currentTouch.current;
				}
			}
			if (currentTouch.dragged == currentTouch.current || (currentScheme != ControlScheme.Controller && currentTouch.clickNotification != 0 && currentTouch.totalDelta.sqrMagnitude < drag))
			{
				if (currentTouch.clickNotification != 0 && currentTouch.pressed == currentTouch.current)
				{
					ShowTooltip(null);
					float time = RealTime.time;
					if (onClick != null)
					{
						onClick(currentTouch.pressed);
					}
					if (currentKey == KeyCode.Mouse1)
					{
						Notify(currentTouch.pressed, "OnRightClick", null);
					}
					else
					{
						Notify(currentTouch.pressed, "OnClick", null);
					}
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
		if (go == null)
		{
			return false;
		}
		Collider component = go.GetComponent<Collider>();
		if (component != null)
		{
			return component.enabled;
		}
		Collider2D component2 = go.GetComponent<Collider2D>();
		if (component2 != null)
		{
			return component2.enabled;
		}
		return false;
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
		if (currentTouch.pressed != null)
		{
			if (released)
			{
				ProcessRelease(flag, num);
			}
			ProcessPress(pressed, num2, num);
			if (currentTouch.deltaTime > tooltipDelay && currentTouch.pressed == currentTouch.current && mTooltipTime != 0f && !currentTouch.dragStarted)
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
		if (mTooltip != go)
		{
			if (mTooltip != null)
			{
				if (onTooltip != null)
				{
					onTooltip(mTooltip, state: false);
				}
				Notify(mTooltip, "OnTooltip", false);
			}
			mTooltip = go;
			mTooltipTime = 0f;
			if (mTooltip != null)
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
