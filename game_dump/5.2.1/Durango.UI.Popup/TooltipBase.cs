using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Popup;

public abstract class TooltipBase : MonoBehaviour
{
	public enum DepthEnum
	{
		Default,
		Over1,
		Over2,
		Over3,
		Floor,
		OverUI
	}

	public enum TooltipDirection
	{
		Horizontal,
		Vertical
	}

	public enum VisibleState
	{
		Wait,
		FadeIn,
		Show,
		Hide
	}

	public enum TriggerType
	{
		Click,
		Hover
	}

	public enum ChangedType
	{
		None,
		Refresh,
		RefreshLayout,
		RefreshLayoutAndPosition
	}

	private static readonly List<TooltipBase> VisibleList;

	protected List<EventDelegate> OnFinished = new List<EventDelegate>();

	protected UISound.GroupType SoundType = UISound.GroupType.PopUp;

	[SerializeField]
	private DepthEnum _depth;

	[SerializeField]
	private bool _alwaysShowFade = true;

	[SerializeField]
	private float _fadeinDelayTime = 0.1f;

	[SerializeField]
	private float _fadeinTime = 0.3f;

	[CanBeNull]
	[SerializeField]
	private UISprite _arrow;

	[SerializeField]
	private float _defaultArrowOffset;

	[SerializeField]
	private float _arrowMargin;

	[SerializeField]
	private TooltipDirection _defaultDiraction;

	[SerializeField]
	private bool _useAutoPosition = true;

	[SerializeField]
	private bool _hideWhenRetouch = true;

	[SerializeField]
	private bool _defaultHideWhenTouch = true;

	[SerializeField]
	private bool _isModal = true;

	[SerializeField]
	private bool _hideWhenTouchModalBg = true;

	[SerializeField]
	private SpriteData _modalBg;

	private VisibleState _state;

	private UIWidget _widget;

	private float _startTime;

	private float _endTime;

	private int _hideFrame;

	private ChangedType _markAsChanged;

	private GameObject _parentObject;

	private bool _isPress;

	public Action OnVisible;

	public DepthEnum Depth
	{
		get
		{
			return _depth;
		}
		set
		{
			_depth = value;
		}
	}

	public VisibleState State
	{
		get
		{
			return _state;
		}
		private set
		{
			if (_state != value)
			{
				_state = value;
				OnChangeState();
			}
		}
	}

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public bool IsModal => _isModal;

	public bool IsVisible { get; private set; }

	public Vector3 TargetPos { get; protected set; }

	protected bool HideWhenTouch
	{
		get
		{
			return _defaultHideWhenTouch;
		}
		set
		{
			_defaultHideWhenTouch = value;
		}
	}

	protected bool HideWhenTouchModalBg
	{
		get
		{
			return _hideWhenTouchModalBg;
		}
		set
		{
			_hideWhenTouchModalBg = value;
		}
	}

	public Transform HideIgnoreParent { get; set; }

	public int Sign { get; set; }

	public TooltipDirection Direction { get; set; }

	public bool AutoPosition { get; set; }

	public virtual bool DragLock { get; set; }

	public bool MuteOpenCloseSound { get; set; }

	protected GameObject ModalBox { get; private set; }

	public static event Action<TooltipBase> ModalOpened;

	public static event Action<TooltipBase> ModalClosed;

	static TooltipBase()
	{
		VisibleList = new List<TooltipBase>();
		GameManager.Reset += delegate
		{
			TooltipBase.ModalOpened = null;
			TooltipBase.ModalClosed = null;
			VisibleList.Clear();
		};
	}

	private void Awake()
	{
		Direction = _defaultDiraction;
		AutoPosition = _useAutoPosition;
		if (_isModal)
		{
			UIWidget uIWidget;
			if (string.IsNullOrEmpty(_modalBg.sprite))
			{
				uIWidget = base.gameObject.AddChild<UIWidget>();
			}
			else
			{
				UISprite uISprite = base.gameObject.AddChild<UISprite>();
				_modalBg.Set(uISprite);
				uIWidget = uISprite;
			}
			uIWidget.depth = Widget.depth - 1;
			uIWidget.autoResizeBoxCollider = true;
			uIWidget.gameObject.AddComponent<BoxCollider>().isTrigger = true;
			uIWidget.leftAnchor.SetScreen(0f, 0f);
			uIWidget.bottomAnchor.SetScreen(0f, 0f);
			uIWidget.rightAnchor.SetScreen(1f, 0f);
			uIWidget.topAnchor.SetScreen(1f, 0f);
			uIWidget.updateAnchors = UIRect.AnchorUpdate.OnUpdate;
			ModalBox = uIWidget.gameObject;
		}
		UIManager.AddOnScreenResized(OnScreenResize);
		OnAwake();
	}

	protected virtual void Start()
	{
		if (!IsVisible)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private static float GetTime()
	{
		return RealTime.time;
	}

	protected virtual void OnScreenResize()
	{
		Hide();
	}

	protected virtual void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	protected virtual void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
		_endTime = 0f;
		ResetArgument();
		Widget.alpha = 1f;
		State = VisibleState.Hide;
	}

	protected virtual void Update()
	{
		if (!IsVisible)
		{
			return;
		}
		if (_markAsChanged != 0)
		{
			switch (_markAsChanged)
			{
			case ChangedType.Refresh:
				Refresh();
				break;
			case ChangedType.RefreshLayout:
				RefreshLayout();
				break;
			case ChangedType.RefreshLayoutAndPosition:
				RefreshLayoutAndPosition();
				break;
			}
			_markAsChanged = ChangedType.None;
		}
		if (_endTime > 0f && _endTime < GetTime())
		{
			Hide();
			return;
		}
		float num = GetTime() - _startTime;
		if (num < _fadeinDelayTime)
		{
			State = VisibleState.Wait;
			Widget.alpha = 0f;
		}
		else if (num < _fadeinTime + _fadeinDelayTime)
		{
			State = VisibleState.FadeIn;
			Widget.alpha = (num - _fadeinDelayTime) / _fadeinTime;
		}
		else
		{
			State = VisibleState.Show;
			Widget.alpha = 1f;
		}
		OnUpdate();
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	private void ResetArgument()
	{
		Direction = _defaultDiraction;
		AutoPosition = _useAutoPosition;
		HideIgnoreParent = null;
		DragLock = false;
		Sign = 0;
		_parentObject = null;
	}

	public void InitializePanelDepth(int depth)
	{
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>(includeInactive: true);
		int i = 0;
		for (int size = KUtility.GetSize(componentsInChildren); i < size; i++)
		{
			componentsInChildren[i].depth += depth;
		}
	}

	public void MarkAsChanged(ChangedType type = ChangedType.Refresh)
	{
		if (IsVisible)
		{
			_markAsChanged = type;
		}
	}

	public void Show()
	{
		Show(0f);
	}

	public void Show(float duration)
	{
		if (!AutoPosition)
		{
			DoShow(Vector3.zero, duration);
			return;
		}
		GameObject selectedObject = UICamera.selectedObject;
		UIWidget uIWidget = ((!(selectedObject == null)) ? selectedObject.GetComponent<UIWidget>() : null);
		if (uIWidget == null)
		{
			Vector3 pos = NGUIMath.ScreenToParentPixels(UICamera.lastEventPosition, base.transform.parent);
			DoShow(pos, duration);
		}
		else
		{
			Show(uIWidget, Vector2.zero, duration);
		}
	}

	public void Show(Vector2 offset, float duration = 0f)
	{
		Show((Transform)null, offset, duration);
	}

	public void Show(GameObject obj, Vector2 offset, float duration = 0f)
	{
		if (!(obj == null))
		{
			UIWidget component = obj.GetComponent<UIWidget>();
			if (component != null)
			{
				Show(component, offset, duration);
			}
			else
			{
				Show(obj.transform, offset, duration);
			}
		}
	}

	public void Show(Transform parent, Vector2 offset, float duration = 0f)
	{
		Vector3 pos = ((!(parent == null)) ? UIUtility.ToRootPosition(parent.gameObject, offset) : ((Vector3)offset));
		DoShow(pos, duration);
	}

	public void Show(UIWidget parent, Vector2 offset, float duration = 0f)
	{
		GameObject selectedObject = UICamera.selectedObject;
		if (_hideWhenRetouch && _parentObject != null && selectedObject == _parentObject)
		{
			if (IsVisible)
			{
				Hide();
			}
			return;
		}
		_parentObject = selectedObject;
		if (Sign == 0)
		{
			Sign = CalcSign(UIUtility.ToRootPosition(parent.gameObject, parent.localCenter), Direction);
		}
		Vector2 vector = new Vector2(0.5f, 0.5f);
		if (Direction == TooltipDirection.Horizontal)
		{
			if (Sign < 0)
			{
				vector = new Vector2(0f, 0.5f);
				offset.x = 0f - offset.x;
			}
			else
			{
				vector = new Vector2(1f, 0.5f);
			}
		}
		else if (Direction == TooltipDirection.Vertical)
		{
			if (Sign < 0)
			{
				vector = new Vector2(0.5f, 0f);
				offset.y = 0f - offset.y;
			}
			else
			{
				vector = new Vector2(0.5f, 1f);
			}
		}
		Rect rect = new Rect(parent.localCorners[0], parent.localSize);
		Vector3 pos2 = UIUtility.ToRootPosition(pos: new Vector3(Mathf.Lerp(rect.xMin, rect.xMax, vector.x), Mathf.Lerp(rect.yMin, rect.yMax, vector.y)), parent: parent.gameObject);
		pos2.x += offset.x;
		pos2.y += offset.y;
		DoShow(pos2, duration);
	}

	public void SetPosition([NotNull] UIWidget target, Vector2 targetPivot, Vector2 pivot, Vector2? arrowOffset = null)
	{
		Vector3[] worldCorners = target.worldCorners;
		Vector2 vector = new Vector2(Mathf.Lerp(worldCorners[0].x, worldCorners[2].x, targetPivot.x), Mathf.Lerp(worldCorners[0].y, worldCorners[2].y, targetPivot.y));
		vector = base.transform.parent.InverseTransformPoint(vector);
		Widget.SetPosition(vector + (arrowOffset.HasValue ? arrowOffset.Value : Vector2.zero), pivot);
		IntoSafeArea();
		if (arrowOffset.HasValue)
		{
			UpdateArrowPosition(vector);
		}
	}

	public void Refresh()
	{
		FillData();
		UpdateButtonShortcut();
		UpdateLayout();
	}

	public void RefreshLayout()
	{
		UpdateLayout();
	}

	public void RefreshLayoutAndPosition()
	{
		UpdateLayout();
		UpdatePosition();
	}

	private void DoShow(Vector3 pos, float duration)
	{
		if (IsShowable())
		{
			TargetPos = pos;
			FillData();
			UpdateButtonShortcut();
			UpdateLayout();
			if (AutoPosition)
			{
				UpdatePosition();
			}
			OnShow();
			SetVisible(duration);
			if (OnVisible != null)
			{
				OnVisible();
			}
		}
	}

	private void SetVisible(float duration)
	{
		bool isVisible = IsVisible;
		if (isVisible)
		{
			ResetArgument();
			EventDelegate.Execute(OnFinished);
		}
		if (isVisible || _hideFrame == Time.frameCount)
		{
			if (_alwaysShowFade)
			{
				_startTime = GetTime();
				Widget.alpha = 0f;
			}
		}
		else
		{
			_startTime = GetTime();
			Widget.alpha = 0f;
		}
		base.gameObject.SetActive(value: true);
		_endTime = ((!(duration > 0f)) ? 0f : (GetTime() + duration));
		IsVisible = true;
	}

	public void Hide(float delay)
	{
		if (delay > 0f)
		{
			_endTime = GetTime() + delay;
		}
		else
		{
			Hide();
		}
	}

	public virtual void Hide()
	{
		if (IsVisible)
		{
			IsVisible = false;
			base.gameObject.SetActive(value: false);
			_hideFrame = Time.frameCount;
			OnHide();
			EventDelegate.Execute(OnFinished);
		}
	}

	public void AddOnFinished(EventDelegate.Callback func)
	{
		EventDelegate.Add(OnFinished, func, oneShot: true);
	}

	public void IntoScreen(int padding = 10)
	{
		Point2 point = new Point2(UIManager.ScreenWidth, UIManager.ScreenHeight);
		Rect rect = new Rect((float)(-point.x) * 0.5f + (float)padding, (float)(-point.y) * 0.5f + (float)padding, point.x - padding * 2, point.y - padding * 2);
		IntoRect(rect);
	}

	public void IntoSafeArea(int padding = 10)
	{
		Point2 point = new Point2(UIManager.ScreenWidth, UIManager.ScreenHeight);
		Rect safeArea = UIManager.SafeArea;
		Rect rect = new Rect((float)(-point.x) * 0.5f + (float)point.x * safeArea.x + (float)padding, (float)(-point.y) * 0.5f + (float)point.y * safeArea.y + (float)padding, (float)point.x * safeArea.width - (float)(padding * 2), (float)point.y * safeArea.height - (float)(padding * 2));
		IntoRect(rect);
	}

	private void IntoRect(Rect rect)
	{
		UIWidget widget = Widget;
		Vector3[] localCorners = widget.localCorners;
		Vector3 vector = Singleton<UIManager>.Instance().UIRoot.transform.InverseTransformPoint(base.transform.parent.position);
		Vector3 localPosition = base.transform.localPosition;
		for (int i = 0; i < localCorners.Length; i++)
		{
			localCorners[i] += vector + localPosition;
		}
		localPosition = Vector3.Lerp(localCorners[0], localCorners[2], 0.5f);
		if (localCorners[0].x < rect.xMin && localCorners[2].x > rect.xMax)
		{
			localPosition.x = 0f;
		}
		else if (localCorners[0].x < rect.xMin)
		{
			localPosition.x = rect.xMin + (float)widget.width * 0.5f;
		}
		else if (localCorners[2].x > rect.xMax)
		{
			localPosition.x = rect.xMax - (float)widget.width * 0.5f;
		}
		if (localCorners[0].y < rect.yMin && localCorners[2].y > rect.yMax)
		{
			localPosition.y = 0f;
		}
		else if (localCorners[0].y < rect.yMin)
		{
			localPosition.y = rect.yMin + (float)widget.height * 0.5f;
		}
		else if (localCorners[2].y > rect.yMax)
		{
			localPosition.y = rect.yMax - (float)widget.height * 0.5f;
		}
		widget.SetPosition(localPosition - vector, 0.5f, 0.5f);
	}

	protected virtual void OnChangeState()
	{
	}

	protected virtual void OnShow()
	{
		VisibleList.Remove(this);
		VisibleList.Add(this);
		if (_isModal && TooltipBase.ModalOpened != null)
		{
			TooltipBase.ModalOpened(this);
		}
		if (!MuteOpenCloseSound && UICamera.hoveredObject == null)
		{
			UISound.PlayOpenGroup(SoundType);
		}
	}

	protected virtual bool IsShowable()
	{
		return true;
	}

	protected virtual void OnHide()
	{
		VisibleList.Remove(this);
		if (_isModal && TooltipBase.ModalClosed != null)
		{
			TooltipBase.ModalClosed(this);
		}
		if (!MuteOpenCloseSound && UICamera.hoveredObject == null)
		{
			UISound.PlayCloseGroup(SoundType);
		}
	}

	protected virtual void FillData()
	{
	}

	private void UpdateButtonShortcut()
	{
		bool showShortcut;
		SelectableButton confirmButton = GetConfirmButton(out showShortcut);
		if (confirmButton != null)
		{
			confirmButton.ShortcutCommand = (showShortcut ? InputCommand.ConfirmModalPopup : InputCommand.None);
		}
		SelectableButton cancelButton = GetCancelButton(out showShortcut);
		if (cancelButton != null)
		{
			cancelButton.ShortcutCommand = (showShortcut ? InputCommand.CancelModalPopup : InputCommand.None);
		}
	}

	protected virtual void UpdateLayout()
	{
	}

	protected virtual void UpdatePosition()
	{
		Vector3 zero = Vector3.zero;
		if (Sign == 0)
		{
			Sign = CalcSign(TargetPos, Direction);
		}
		Vector2 pivotOffset = Widget.pivotOffset;
		float num = ((!(_arrow == null)) ? _arrow.width : 0);
		float min = ((!(_arrow == null)) ? _arrow.height : 0);
		Vector2 vector = Vector2.one * 0.5f;
		Vector2 zero2 = Vector2.zero;
		if (Direction == TooltipDirection.Horizontal)
		{
			vector.x = ((Sign <= 0) ? 1f : 0f);
			zero2.x = num * (float)Sign;
		}
		else if (Direction == TooltipDirection.Vertical && _defaultArrowOffset > 0f)
		{
			vector.x = 0f;
			zero2.x = 0f - Mathf.Clamp(_defaultArrowOffset, min, (float)Widget.width * 0.5f);
		}
		if (Direction == TooltipDirection.Horizontal)
		{
			if (_defaultArrowOffset > 0f)
			{
				vector.y = 1f;
				zero2.y = Mathf.Clamp(_defaultArrowOffset, min, (float)Widget.height * 0.5f);
			}
		}
		else if (Direction == TooltipDirection.Vertical)
		{
			vector.y = ((Sign <= 0) ? 1f : 0f);
			zero2.y = num * (float)Sign;
		}
		Vector2 vector2 = pivotOffset - vector;
		zero2.x += vector2.x * (float)Widget.width;
		zero2.y += vector2.y * (float)Widget.height;
		zero.x = TargetPos.x + zero2.x;
		zero.y = TargetPos.y + zero2.y;
		base.transform.localPosition = zero;
		IntoSafeArea();
		UpdateArrowPosition(TargetPos);
		Sign = 0;
		Direction = _defaultDiraction;
	}

	public void UpdateArrowPosition(Vector3 targetPos)
	{
		if (_arrow == null)
		{
			return;
		}
		UIWidget widget = Widget;
		Rect rect = new Rect(widget.GetPosition(0f, 0f), widget.localSize);
		float num = (float)_arrow.width - _arrowMargin;
		float num2 = (float)_arrow.height * 0.5f;
		Rect rect2 = new Rect(rect.position - Vector2.one * num, rect.size + Vector2.one * num * 2f);
		Rect rect3 = new Rect(rect.position + Vector2.one * num2, rect.size - Vector2.one * num2 * 2f);
		rect2.position += Vector2.one;
		rect2.size -= Vector2.one;
		if (!rect2.Contains(targetPos))
		{
			if (rect3.xMin < targetPos.x && targetPos.x < rect3.xMax)
			{
				if (targetPos.y < rect.yMin)
				{
					_arrow.transform.localPosition = new Vector3(targetPos.x, rect.yMin) - base.transform.localPosition;
					_arrow.transform.localEulerAngles = Vector3.forward * 270f;
				}
				else
				{
					_arrow.transform.localPosition = new Vector3(targetPos.x, rect.yMax) - base.transform.localPosition;
					_arrow.transform.localEulerAngles = Vector3.forward * 90f;
				}
				_arrow.gameObject.SetActive(value: true);
				return;
			}
			if (rect3.yMin < targetPos.y && targetPos.y < rect3.yMax)
			{
				if (targetPos.x < rect.xMin)
				{
					_arrow.transform.localPosition = new Vector3(rect.xMin, targetPos.y) - base.transform.localPosition;
					_arrow.transform.localEulerAngles = Vector3.forward * 180f;
				}
				else
				{
					_arrow.transform.localPosition = new Vector3(rect.xMax, targetPos.y) - base.transform.localPosition;
					_arrow.transform.localEulerAngles = Vector3.forward * 0f;
				}
				_arrow.gameObject.SetActive(value: true);
				return;
			}
		}
		_arrow.gameObject.SetActive(value: false);
	}

	private static int CalcSign(Vector3 pos, TooltipDirection direction)
	{
		if (direction == TooltipDirection.Horizontal)
		{
			if (pos.x > 0f)
			{
				return -1;
			}
			return 1;
		}
		if (pos.y > 0f)
		{
			return -1;
		}
		return 1;
	}

	public void HideArrow()
	{
		if (_arrow != null)
		{
			_arrow.gameObject.SetActive(value: false);
		}
	}

	protected void OnDrag(Vector2 delta)
	{
		_isPress = false;
		if (!DragLock)
		{
			Transform obj = base.transform;
			Vector3 localPosition = obj.localPosition;
			localPosition.x += delta.x;
			localPosition.y += delta.y;
			obj.localPosition = localPosition;
			OnMoveWidget();
		}
	}

	protected void OnPress(bool press)
	{
		if (press)
		{
			_isPress = true;
		}
		else if (_isPress)
		{
			OnClickWidget();
		}
	}

	protected virtual void OnMoveWidget()
	{
		HideArrow();
	}

	protected virtual void OnClickWidget()
	{
	}

	private void OnTouch(GameObject touchObj, bool press)
	{
		if (!press || !IsVisible)
		{
			return;
		}
		if (_isModal && ModalBox == touchObj && _hideWhenTouchModalBg)
		{
			Hide();
		}
		else if (!_isModal || IsTopMostModal())
		{
			Transform child = ((!(touchObj == null)) ? touchObj.transform : null);
			if (HideWhenTouch && !NGUITools.IsChild(base.transform, child) && !NGUITools.IsChild(HideIgnoreParent, child))
			{
				Hide();
			}
		}
	}

	private bool IsTopMostModal()
	{
		if (_isModal)
		{
			return VisibleList.LastOrDefault((TooltipBase x) => x._isModal) == this;
		}
		return false;
	}

	public static bool HasModal()
	{
		foreach (TooltipBase visible in VisibleList)
		{
			if (visible._isModal)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Close()
	{
		for (int num = VisibleList.Count - 1; num >= 0; num--)
		{
			TooltipBase tooltipBase = VisibleList[num];
			if (tooltipBase.IsVisible)
			{
				tooltipBase.Hide();
				return true;
			}
			VisibleList.RemoveAt(num);
		}
		return false;
	}

	public static void CloseAll()
	{
		for (int i = 0; i < 100; i++)
		{
			if (!Close())
			{
				break;
			}
		}
	}

	public static void TryConfirmOnModal(InputCommandMessage message)
	{
		TooltipBase tooltipBase = VisibleList.LastOrDefault((TooltipBase x) => x._isModal);
		if (!(tooltipBase == null))
		{
			tooltipBase.OnTryConfirmOnModal();
		}
	}

	public static void TryCancelOnModal(InputCommandMessage message)
	{
		TooltipBase tooltipBase = VisibleList.LastOrDefault((TooltipBase x) => x._isModal);
		if (!(tooltipBase == null))
		{
			tooltipBase.OnTryCancelOnModal();
		}
	}

	protected virtual void OnTryConfirmOnModal()
	{
	}

	protected virtual void OnTryCancelOnModal()
	{
		if (_hideWhenTouchModalBg)
		{
			Hide();
		}
	}

	protected virtual SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = false;
		return null;
	}

	protected virtual SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = false;
		return null;
	}

	public static UIEventListener.BoolDelegate ToHover(Func<GameObject, TooltipBase> onHover)
	{
		TooltipBase tooltip = null;
		return delegate(GameObject go, bool isHover)
		{
			if (isHover)
			{
				tooltip = onHover(go);
			}
			else if (tooltip != null)
			{
				tooltip.Hide();
				tooltip = null;
			}
		};
	}
}
