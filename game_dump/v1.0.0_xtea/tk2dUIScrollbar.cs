using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/UI/tk2dUIScrollbar")]
public class tk2dUIScrollbar : MonoBehaviour
{
	public enum Axes
	{
		XAxis,
		YAxis
	}

	private const float WITHOUT_SCROLLBAR_FIXED_SCROLL_WHEEL_PERCENT = 0.1f;

	private const float INITIAL_TIME_TO_REPEAT_UP_DOWN_SCROLL_BUTTON_SCROLLING_ON_HOLD = 0.55f;

	private const float TIME_TO_REPEAT_UP_DOWN_SCROLL_BUTTON_SCROLLING_ON_HOLD = 0.45f;

	public tk2dUIItem barUIItem;

	public float scrollBarLength;

	public tk2dUIItem thumbBtn;

	public Transform thumbTransform;

	public float thumbLength;

	public tk2dUIItem upButton;

	private tk2dUIHoverItem hoverUpButton;

	public tk2dUIItem downButton;

	private tk2dUIHoverItem hoverDownButton;

	public float buttonUpDownScrollDistance = 1f;

	public bool allowScrollWheel = true;

	public Axes scrollAxes = Axes.YAxis;

	public tk2dUIProgressBar highlightProgressBar;

	[SerializeField]
	[HideInInspector]
	private tk2dUILayout barLayoutItem;

	private bool isScrollThumbButtonDown;

	private bool isTrackHoverOver;

	private float percent;

	private Vector3 moveThumbBtnOffset = Vector3.zero;

	private int scrollUpDownButtonState;

	private float timeOfUpDownButtonPressStart;

	private float repeatUpDownButtonHoldCounter;

	public string SendMessageOnScrollMethodName = string.Empty;

	public tk2dUILayout BarLayoutItem
	{
		get
		{
			return barLayoutItem;
		}
		set
		{
			if ((Object)(object)barLayoutItem != (Object)(object)value)
			{
				if ((Object)(object)barLayoutItem != (Object)null)
				{
					barLayoutItem.OnReshape -= LayoutReshaped;
				}
				barLayoutItem = value;
				if ((Object)(object)barLayoutItem != (Object)null)
				{
					barLayoutItem.OnReshape += LayoutReshaped;
				}
			}
		}
	}

	public GameObject SendMessageTarget
	{
		get
		{
			if ((Object)(object)barUIItem != (Object)null)
			{
				return barUIItem.sendMessageTarget;
			}
			return null;
		}
		set
		{
			if ((Object)(object)barUIItem != (Object)null && (Object)(object)barUIItem.sendMessageTarget != (Object)(object)value)
			{
				barUIItem.sendMessageTarget = value;
			}
		}
	}

	public float Value
	{
		get
		{
			return percent;
		}
		set
		{
			percent = Mathf.Clamp(value, 0f, 1f);
			if (this.OnScroll != null)
			{
				this.OnScroll(this);
			}
			SetScrollThumbPosition();
			if ((Object)(object)SendMessageTarget != (Object)null && SendMessageOnScrollMethodName.Length > 0)
			{
				SendMessageTarget.SendMessage(SendMessageOnScrollMethodName, (object)this, (SendMessageOptions)0);
			}
		}
	}

	public event Action<tk2dUIScrollbar> OnScroll;

	public void SetScrollPercentWithoutEvent(float newScrollPercent)
	{
		percent = Mathf.Clamp(newScrollPercent, 0f, 1f);
		SetScrollThumbPosition();
	}

	private void OnEnable()
	{
		if ((Object)(object)barUIItem != (Object)null)
		{
			barUIItem.OnDown += ScrollTrackButtonDown;
			barUIItem.OnHoverOver += ScrollTrackButtonHoverOver;
			barUIItem.OnHoverOut += ScrollTrackButtonHoverOut;
		}
		if ((Object)(object)thumbBtn != (Object)null)
		{
			thumbBtn.OnDown += ScrollThumbButtonDown;
			thumbBtn.OnRelease += ScrollThumbButtonRelease;
		}
		if ((Object)(object)upButton != (Object)null)
		{
			upButton.OnDown += ScrollUpButtonDown;
			upButton.OnUp += ScrollUpButtonUp;
		}
		if ((Object)(object)downButton != (Object)null)
		{
			downButton.OnDown += ScrollDownButtonDown;
			downButton.OnUp += ScrollDownButtonUp;
		}
		if ((Object)(object)barLayoutItem != (Object)null)
		{
			barLayoutItem.OnReshape += LayoutReshaped;
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)barUIItem != (Object)null)
		{
			barUIItem.OnDown -= ScrollTrackButtonDown;
			barUIItem.OnHoverOver -= ScrollTrackButtonHoverOver;
			barUIItem.OnHoverOut -= ScrollTrackButtonHoverOut;
		}
		if ((Object)(object)thumbBtn != (Object)null)
		{
			thumbBtn.OnDown -= ScrollThumbButtonDown;
			thumbBtn.OnRelease -= ScrollThumbButtonRelease;
		}
		if ((Object)(object)upButton != (Object)null)
		{
			upButton.OnDown -= ScrollUpButtonDown;
			upButton.OnUp -= ScrollUpButtonUp;
		}
		if ((Object)(object)downButton != (Object)null)
		{
			downButton.OnDown -= ScrollDownButtonDown;
			downButton.OnUp -= ScrollDownButtonUp;
		}
		if (isScrollThumbButtonDown)
		{
			if ((Object)(object)tk2dUIManager.Instance__NoCreate != (Object)null)
			{
				tk2dUIManager.Instance.OnInputUpdate -= MoveScrollThumbButton;
			}
			isScrollThumbButtonDown = false;
		}
		if (isTrackHoverOver)
		{
			if ((Object)(object)tk2dUIManager.Instance__NoCreate != (Object)null)
			{
				tk2dUIManager.Instance.OnScrollWheelChange -= TrackHoverScrollWheelChange;
			}
			isTrackHoverOver = false;
		}
		if (scrollUpDownButtonState != 0)
		{
			tk2dUIManager.Instance.OnInputUpdate -= CheckRepeatScrollUpDownButton;
			scrollUpDownButtonState = 0;
		}
		if ((Object)(object)barLayoutItem != (Object)null)
		{
			barLayoutItem.OnReshape -= LayoutReshaped;
		}
	}

	private void Awake()
	{
		if ((Object)(object)upButton != (Object)null)
		{
			hoverUpButton = ((Component)upButton).GetComponent<tk2dUIHoverItem>();
		}
		if ((Object)(object)downButton != (Object)null)
		{
			hoverDownButton = ((Component)downButton).GetComponent<tk2dUIHoverItem>();
		}
	}

	private void Start()
	{
		SetScrollThumbPosition();
	}

	private void TrackHoverScrollWheelChange(float mouseWheelChange)
	{
		if (mouseWheelChange > 0f)
		{
			ScrollUpFixed();
		}
		else if (mouseWheelChange < 0f)
		{
			ScrollDownFixed();
		}
	}

	private void SetScrollThumbPosition()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)thumbTransform != (Object)null)
		{
			float num = 0f - (scrollBarLength - thumbLength) * Value;
			Vector3 localPosition = thumbTransform.localPosition;
			if (scrollAxes == Axes.XAxis)
			{
				localPosition.x = 0f - num;
			}
			else if (scrollAxes == Axes.YAxis)
			{
				localPosition.y = num;
			}
			thumbTransform.localPosition = localPosition;
		}
		if ((Object)(object)highlightProgressBar != (Object)null)
		{
			highlightProgressBar.Value = Value;
		}
	}

	private void MoveScrollThumbButton()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		ScrollToPosition(CalculateClickWorldPos(thumbBtn) + moveThumbBtnOffset);
	}

	private Vector3 CalculateClickWorldPos(tk2dUIItem btn)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		Camera uICameraForControl = tk2dUIManager.Instance.GetUICameraForControl(((Component)this).gameObject);
		Vector2 position = btn.Touch.position;
		Vector3 result = uICameraForControl.ScreenToWorldPoint(new Vector3(position.x, position.y, ((Component)btn).transform.position.z - ((Component)uICameraForControl).transform.position.z));
		result.z = ((Component)btn).transform.position.z;
		return result;
	}

	private void ScrollToPosition(Vector3 worldPos)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = thumbTransform.parent.InverseTransformPoint(worldPos);
		float num = 0f;
		if (scrollAxes == Axes.XAxis)
		{
			num = val.x;
		}
		else if (scrollAxes == Axes.YAxis)
		{
			num = 0f - val.y;
		}
		Value = num / (scrollBarLength - thumbLength);
	}

	private void ScrollTrackButtonDown()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		ScrollToPosition(CalculateClickWorldPos(barUIItem));
	}

	private void ScrollTrackButtonHoverOver()
	{
		if (allowScrollWheel)
		{
			if (!isTrackHoverOver)
			{
				tk2dUIManager.Instance.OnScrollWheelChange += TrackHoverScrollWheelChange;
			}
			isTrackHoverOver = true;
		}
	}

	private void ScrollTrackButtonHoverOut()
	{
		if (isTrackHoverOver)
		{
			tk2dUIManager.Instance.OnScrollWheelChange -= TrackHoverScrollWheelChange;
		}
		isTrackHoverOver = false;
	}

	private void ScrollThumbButtonDown()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!isScrollThumbButtonDown)
		{
			tk2dUIManager.Instance.OnInputUpdate += MoveScrollThumbButton;
		}
		isScrollThumbButtonDown = true;
		Vector3 val = CalculateClickWorldPos(thumbBtn);
		moveThumbBtnOffset = ((Component)thumbBtn).transform.position - val;
		moveThumbBtnOffset.z = 0f;
		if ((Object)(object)hoverUpButton != (Object)null)
		{
			hoverUpButton.IsOver = true;
		}
		if ((Object)(object)hoverDownButton != (Object)null)
		{
			hoverDownButton.IsOver = true;
		}
	}

	private void ScrollThumbButtonRelease()
	{
		if (isScrollThumbButtonDown)
		{
			tk2dUIManager.Instance.OnInputUpdate -= MoveScrollThumbButton;
		}
		isScrollThumbButtonDown = false;
		if ((Object)(object)hoverUpButton != (Object)null)
		{
			hoverUpButton.IsOver = false;
		}
		if ((Object)(object)hoverDownButton != (Object)null)
		{
			hoverDownButton.IsOver = false;
		}
	}

	private void ScrollUpButtonDown()
	{
		timeOfUpDownButtonPressStart = Time.realtimeSinceStartup;
		repeatUpDownButtonHoldCounter = 0f;
		if (scrollUpDownButtonState == 0)
		{
			tk2dUIManager.Instance.OnInputUpdate += CheckRepeatScrollUpDownButton;
		}
		scrollUpDownButtonState = -1;
		ScrollUpFixed();
	}

	private void ScrollUpButtonUp()
	{
		if (scrollUpDownButtonState != 0)
		{
			tk2dUIManager.Instance.OnInputUpdate -= CheckRepeatScrollUpDownButton;
		}
		scrollUpDownButtonState = 0;
	}

	private void ScrollDownButtonDown()
	{
		timeOfUpDownButtonPressStart = Time.realtimeSinceStartup;
		repeatUpDownButtonHoldCounter = 0f;
		if (scrollUpDownButtonState == 0)
		{
			tk2dUIManager.Instance.OnInputUpdate += CheckRepeatScrollUpDownButton;
		}
		scrollUpDownButtonState = 1;
		ScrollDownFixed();
	}

	private void ScrollDownButtonUp()
	{
		if (scrollUpDownButtonState != 0)
		{
			tk2dUIManager.Instance.OnInputUpdate -= CheckRepeatScrollUpDownButton;
		}
		scrollUpDownButtonState = 0;
	}

	public void ScrollUpFixed()
	{
		ScrollDirection(-1);
	}

	public void ScrollDownFixed()
	{
		ScrollDirection(1);
	}

	private void CheckRepeatScrollUpDownButton()
	{
		if (scrollUpDownButtonState == 0)
		{
			return;
		}
		float num = Time.realtimeSinceStartup - timeOfUpDownButtonPressStart;
		if (repeatUpDownButtonHoldCounter == 0f)
		{
			if (num > 0.55f)
			{
				repeatUpDownButtonHoldCounter += 1f;
				num -= 0.55f;
				ScrollDirection(scrollUpDownButtonState);
			}
		}
		else if (num > 0.45f)
		{
			repeatUpDownButtonHoldCounter += 1f;
			num -= 0.45f;
			ScrollDirection(scrollUpDownButtonState);
		}
	}

	public void ScrollDirection(int dir)
	{
		if (scrollAxes == Axes.XAxis)
		{
			Value -= CalcScrollPercentOffsetButtonScrollDistance() * (float)dir * buttonUpDownScrollDistance;
		}
		else
		{
			Value += CalcScrollPercentOffsetButtonScrollDistance() * (float)dir * buttonUpDownScrollDistance;
		}
	}

	private float CalcScrollPercentOffsetButtonScrollDistance()
	{
		return 0.1f;
	}

	private void LayoutReshaped(Vector3 dMin, Vector3 dMax)
	{
		scrollBarLength += ((scrollAxes != 0) ? (dMax.y - dMin.y) : (dMax.x - dMin.x));
	}
}
