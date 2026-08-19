using System;
using System.Collections;
using UnityEngine;

public abstract class TooltipBase : TimeLimitWidget
{
	public enum TooltipDirection
	{
		Horizontal,
		Vertical
	}

	public Action TouchedMe;

	[SerializeField]
	private UISprite _arrow;

	[SerializeField]
	private float _defaultArrowOffset;

	[SerializeField]
	private float _screenMargin = 10f;

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

	private bool _changeFlag;

	private GameObject _parentObject;

	private bool _isPress;

	protected Vector3 TargetPos { get; private set; }

	public bool HideWhenTouch { get; set; }

	public Transform HideIgnoreParent { get; set; }

	public int Sign { get; set; }

	public TooltipDirection Direction { get; set; }

	public bool AutoPosition { get; set; }

	public virtual bool DragLock { get; set; }

	private void Awake()
	{
		((Component)this).gameObject.SetActive(false);
		HideWhenTouch = _defaultHideWhenTouch;
		Direction = _defaultDiraction;
		AutoPosition = _useAutoPosition;
		OnAwake();
	}

	protected virtual void OnPortraitMode(bool isPortraitMode)
	{
		Hide(instant: true);
	}

	protected virtual void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	protected virtual void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	protected virtual void OnAwake()
	{
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Input.GetKeyDown((KeyCode)27))
		{
			Hide();
		}
	}

	protected override void OnFinish()
	{
		base.OnFinish();
		HideWhenTouch = _defaultHideWhenTouch;
		Direction = _defaultDiraction;
		AutoPosition = _useAutoPosition;
		HideIgnoreParent = null;
		TouchedMe = null;
		DragLock = false;
		_parentObject = null;
	}

	public void MarkAsChange()
	{
		if (base.IsVisible && !_changeFlag)
		{
			_changeFlag = true;
			((MonoBehaviour)this).StartCoroutine(CoLateUpdate());
		}
	}

	private IEnumerator CoLateUpdate()
	{
		yield return null;
		_changeFlag = false;
		Refresh();
	}

	public void Show()
	{
		Show(3600f);
	}

	public void Show(float duration)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		GameObject selectedObject = UICamera.selectedObject;
		UIWidget uIWidget = ((!((Object)(object)selectedObject == (Object)null)) ? selectedObject.GetComponent<UIWidget>() : null);
		if ((Object)(object)uIWidget == (Object)null)
		{
			Vector3 pos = Vector2.op_Implicit(NGUIMath.ScreenToParentPixels(UICamera.lastEventPosition, ((Component)this).transform.parent));
			Show(pos, duration);
		}
		else
		{
			Show(uIWidget, Vector2.zero, duration);
		}
	}

	public void Show(Vector2 offset, float duration)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Show((Transform)null, offset, duration);
	}

	public void Show(GameObject obj, Vector2 offset, float duration)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)obj == (Object)null))
		{
			UIWidget component = obj.GetComponent<UIWidget>();
			if ((Object)(object)component != (Object)null)
			{
				Show(component, offset, duration);
			}
			else
			{
				Show(obj.transform, offset, duration);
			}
		}
	}

	public void Show(Transform parent, Vector2 offset, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 pos = MainCamera.NGUILocalPositionToNGUIPosition(Vector2.op_Implicit(offset), parent);
		Show(pos, duration);
	}

	public void Show(UIWidget parent, Vector2 offset, float duration)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		GameObject selectedObject = UICamera.selectedObject;
		if (_hideWhenRetouch && (Object)(object)_parentObject != (Object)null && (Object)(object)selectedObject == (Object)(object)_parentObject)
		{
			if (base.IsVisible)
			{
				Hide();
			}
			return;
		}
		_parentObject = selectedObject;
		Transform transform = ((Component)parent).transform;
		Vector3 localCenter = parent.localCenter;
		Vector3 zero = Vector3.zero;
		Vector3 val = MainCamera.NGUILocalPositionToNGUIPosition(localCenter, transform);
		if (Sign == 0)
		{
			Sign = CalcSign(val, Direction);
		}
		if (Direction == TooltipDirection.Horizontal)
		{
			if (Sign < 0)
			{
				zero.x = (float)(-parent.width) * 0.5f;
				offset.x = 0f - offset.x;
			}
			else
			{
				zero.x = (float)parent.width * 0.5f;
			}
		}
		else if (Direction == TooltipDirection.Vertical)
		{
			if (Sign < 0)
			{
				zero.y = (float)(-parent.height) * 0.5f;
				offset.y = 0f - offset.y;
			}
			else
			{
				zero.y = (float)parent.height * 0.5f;
			}
		}
		zero.x += offset.x;
		zero.y += offset.y;
		Show(val + zero, duration);
	}

	public void Refresh()
	{
		FillData();
		UpdateLayout();
		VisibleTimeReset();
	}

	private void Show(Vector3 pos, float duration)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		TargetPos = pos;
		OnShow();
		FillData();
		UpdateLayout();
		if (AutoPosition)
		{
			UpdatePosition();
		}
		Visible(duration);
	}

	protected virtual void OnShow()
	{
	}

	protected abstract void FillData();

	protected abstract void UpdateLayout();

	private void UpdatePosition()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zero = Vector3.zero;
		if (Sign == 0)
		{
			Sign = CalcSign(TargetPos, Direction);
		}
		Vector2 pivotOffset = base.Widget.pivotOffset;
		float num = ((!((Object)(object)_arrow == (Object)null)) ? _arrow.width : 0);
		float num2 = ((!((Object)(object)_arrow == (Object)null)) ? _arrow.height : 0);
		float num3 = (float)base.Widget.width * 0.5f;
		if (Direction == TooltipDirection.Horizontal)
		{
			num3 += num;
			num3 *= (float)Sign;
		}
		else if (Direction == TooltipDirection.Vertical)
		{
			num3 -= Mathf.Clamp(_defaultArrowOffset, num2, (float)base.Widget.width / 2f);
		}
		num3 += (float)base.Widget.width * (pivotOffset.x - 0.5f);
		float num4 = 0f;
		if (Direction == TooltipDirection.Horizontal)
		{
			num4 = (float)(-base.Widget.height) * 0.5f + Mathf.Clamp(_defaultArrowOffset, num2, (float)base.Widget.height / 2f);
		}
		else if (Direction == TooltipDirection.Vertical)
		{
			num4 = (float)base.Widget.height * 0.5f + num;
			num4 *= (float)Sign;
		}
		num4 += (float)base.Widget.height * (pivotOffset.y - 0.5f);
		zero.x = TargetPos.x + num3;
		zero.y = TargetPos.y + num4;
		float num5 = ((float)UIManager.ScreenWidth - _screenMargin * 2f) / 2f;
		float num6 = ((float)UIManager.ScreenHeight - _screenMargin * 2f) / 2f;
		if ((float)base.Widget.height > num6 * 2f || zero.y + (float)base.Widget.height * (1f - pivotOffset.y) > num6)
		{
			zero.y = num6 - (float)base.Widget.height * (1f - pivotOffset.y);
			HideArrow();
		}
		else if (zero.y - (float)base.Widget.height * pivotOffset.y < 0f - num6)
		{
			zero.y = 0f - num6 + (float)base.Widget.height * pivotOffset.y;
			HideArrow();
		}
		if ((float)base.Widget.width > num5 * 2f || zero.x - (float)base.Widget.width * pivotOffset.x < 0f - num5)
		{
			zero.x = 0f - num5 + (float)base.Widget.width * pivotOffset.x;
			HideArrow();
		}
		else if (zero.x + (float)base.Widget.width * (1f - pivotOffset.x) > num5)
		{
			zero.x = num5 - (float)base.Widget.width * (1f - pivotOffset.x);
			HideArrow();
		}
		((Component)this).transform.localPosition = zero;
		UpdateArrowPosition();
		Sign = 0;
		Direction = _defaultDiraction;
	}

	protected void UpdateArrowPosition()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_arrow == (Object)null))
		{
			Vector3 localPosition = ((Component)this).transform.localPosition;
			Vector3 val = TargetPos - localPosition;
			int num = 0;
			float num2 = (float)_arrow.width - _arrowMargin;
			if (Direction == TooltipDirection.Horizontal)
			{
				val += Vector3.right * (num2 * (float)Sign);
				num = 90 + 90 * Sign;
			}
			else if (Direction == TooltipDirection.Vertical)
			{
				val += Vector3.up * (num2 * (float)Sign);
				num = -90 * Sign;
			}
			Transform transform = ((Component)_arrow).transform;
			transform.localPosition = val;
			transform.localEulerAngles = Vector3.forward * (float)num;
			((Component)_arrow).gameObject.SetActive(true);
		}
	}

	private static int CalcSign(Vector3 pos, TooltipDirection direction)
	{
		int num = 0;
		if (direction == TooltipDirection.Horizontal)
		{
			return (!(pos.x > 0f)) ? 1 : (-1);
		}
		return (!(pos.y > 0f)) ? 1 : (-1);
	}

	public void HideArrow()
	{
		if ((Object)(object)_arrow != (Object)null)
		{
			((Component)_arrow).gameObject.SetActive(false);
		}
	}

	protected void OnDrag(Vector2 delta)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_isPress = false;
		VisibleTimeReset();
		if (!DragLock)
		{
			Transform transform = ((Component)this).transform;
			Vector3 localPosition = transform.localPosition;
			localPosition.x += delta.x;
			localPosition.y += delta.y;
			transform.localPosition = localPosition;
			OnMoveWidget();
		}
	}

	protected void OnPress(bool press)
	{
		if (press)
		{
			_isPress = true;
			VisibleTimeReset();
		}
		else if (_isPress)
		{
			OnClickWidget();
		}
		else
		{
			VisibleTimeReset();
		}
	}

	protected virtual void OnMoveWidget()
	{
		HideArrow();
	}

	protected virtual void OnClickWidget()
	{
		Hide();
	}

	private void OnTouch(GameObject touchObj, bool press)
	{
		if (!press || !base.IsVisible)
		{
			return;
		}
		Transform child = ((!((Object)(object)touchObj == (Object)null)) ? touchObj.transform : null);
		if (NGUITools.IsChild(((Component)this).transform, child))
		{
			VisibleTimeReset();
			if (TouchedMe != null)
			{
				TouchedMe();
			}
		}
		else if (!NGUITools.IsChild(HideIgnoreParent, child))
		{
			if (HideWhenTouch)
			{
				Hide();
			}
		}
		else
		{
			VisibleTimeReset();
		}
	}
}
