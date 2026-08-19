using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Popup List")]
public class UIPopupList : UIWidgetContainer
{
	public enum Position
	{
		Auto,
		Above,
		Below
	}

	public enum OpenOn
	{
		ClickOrTap,
		RightClick,
		DoubleClick,
		Manual
	}

	public delegate void LegacyEvent(string val);

	[CompilerGenerated]
	private sealed class _003CCloseIfUnselected_003Ed__103 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public UIPopupList _003C_003E4__this;

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
		public _003CCloseIfUnselected_003Ed__103(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			UIPopupList uIPopupList = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				if (UICamera.selectedObject != uIPopupList.mSelection)
				{
					uIPopupList.CloseSelf();
					return false;
				}
			}
			else
			{
				_003C_003E1__state = -1;
			}
			_003C_003E2__current = null;
			_003C_003E1__state = 1;
			return true;
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
	private sealed class _003CUpdateTweenPosition_003Ed__87 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public UIPopupList _003C_003E4__this;

		private TweenPosition _003Ctp_003E5__2;

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
		public _003CUpdateTweenPosition_003Ed__87(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Ctp_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			UIPopupList uIPopupList = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
			}
			else
			{
				_003C_003E1__state = -1;
				if (!(uIPopupList.mHighlight != null) || !(uIPopupList.mHighlightedLabel != null))
				{
					goto IL_0097;
				}
				_003Ctp_003E5__2 = uIPopupList.mHighlight.GetComponent<TweenPosition>();
			}
			if (_003Ctp_003E5__2 != null && _003Ctp_003E5__2.enabled)
			{
				_003Ctp_003E5__2.to = uIPopupList.GetHighlightPosition();
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			_003Ctp_003E5__2 = null;
			goto IL_0097;
			IL_0097:
			uIPopupList.mTweening = false;
			return false;
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

	public static UIPopupList current;

	private static GameObject mChild;

	private static float mFadeOutComplete;

	private const float animSpeed = 0.15f;

	public UIAtlas atlas;

	public UIFont bitmapFont;

	public Font trueTypeFont;

	public int fontSize = 16;

	public FontStyle fontStyle;

	public string backgroundSprite;

	public string highlightSprite;

	public Sprite background2DSprite;

	public Sprite highlight2DSprite;

	public Position position;

	public NGUIText.Alignment alignment = NGUIText.Alignment.Left;

	public List<string> items = new List<string>();

	public List<object> itemData = new List<object>();

	public Vector2 padding = new Vector3(4f, 4f);

	public Color textColor = Color.white;

	public Color backgroundColor = Color.white;

	public Color highlightColor = new Color(0.88235295f, 40f / 51f, 0.5882353f, 1f);

	public bool isAnimated = true;

	public bool isLocalized;

	public bool separatePanel = true;

	public OpenOn openOn;

	public List<EventDelegate> onChange = new List<EventDelegate>();

	[HideInInspector]
	[SerializeField]
	protected string mSelectedItem;

	[HideInInspector]
	[SerializeField]
	protected UIPanel mPanel;

	[HideInInspector]
	[SerializeField]
	protected UIBasicSprite mBackground;

	[HideInInspector]
	[SerializeField]
	protected UIBasicSprite mHighlight;

	[HideInInspector]
	[SerializeField]
	protected UILabel mHighlightedLabel;

	[HideInInspector]
	[SerializeField]
	protected List<UILabel> mLabelList = new List<UILabel>();

	[HideInInspector]
	[SerializeField]
	protected float mBgBorder;

	[NonSerialized]
	protected GameObject mSelection;

	[NonSerialized]
	protected int mOpenFrame;

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[HideInInspector]
	[SerializeField]
	private string functionName = "OnSelectionChange";

	[HideInInspector]
	[SerializeField]
	private float textScale;

	[HideInInspector]
	[SerializeField]
	private UIFont font;

	[HideInInspector]
	[SerializeField]
	private UILabel textLabel;

	[NonSerialized]
	public Vector3 startingPosition;

	private LegacyEvent mLegacyEvent;

	[NonSerialized]
	protected bool mExecuting;

	protected bool mUseDynamicFont;

	[NonSerialized]
	protected bool mStarted;

	protected bool mTweening;

	public GameObject source;

	public UnityEngine.Object ambigiousFont
	{
		get
		{
			if (trueTypeFont != null)
			{
				return trueTypeFont;
			}
			if (bitmapFont != null)
			{
				return bitmapFont;
			}
			return font;
		}
		set
		{
			if (value is Font)
			{
				trueTypeFont = value as Font;
				bitmapFont = null;
				font = null;
			}
			else if (value is UIFont)
			{
				bitmapFont = value as UIFont;
				trueTypeFont = null;
				font = null;
			}
		}
	}

	[Obsolete("Use EventDelegate.Add(popup.onChange, YourCallback) instead, and UIPopupList.current.value to determine the state")]
	public LegacyEvent onSelectionChange
	{
		get
		{
			return mLegacyEvent;
		}
		set
		{
			mLegacyEvent = value;
		}
	}

	public static bool isOpen
	{
		get
		{
			if (current != null)
			{
				if (!(mChild != null))
				{
					return mFadeOutComplete > Time.unscaledTime;
				}
				return true;
			}
			return false;
		}
	}

	public virtual string value
	{
		get
		{
			return mSelectedItem;
		}
		set
		{
			Set(value);
		}
	}

	public virtual object data
	{
		get
		{
			int num = items.IndexOf(mSelectedItem);
			if (num > -1 && num < itemData.Count)
			{
				return itemData[num];
			}
			return null;
		}
	}

	public bool isColliderEnabled
	{
		get
		{
			Collider component = GetComponent<Collider>();
			if (component != null)
			{
				return component.enabled;
			}
			Collider2D component2 = GetComponent<Collider2D>();
			if (component2 != null)
			{
				return component2.enabled;
			}
			return false;
		}
	}

	[Obsolete("Use 'value' instead")]
	public string selection
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	private bool isValid
	{
		get
		{
			if (!(bitmapFont != null))
			{
				return trueTypeFont != null;
			}
			return true;
		}
	}

	private int activeFontSize
	{
		get
		{
			if (trueTypeFont != null || bitmapFont == null)
			{
				return fontSize;
			}
			return bitmapFont.defaultSize;
		}
	}

	private float activeFontScale
	{
		get
		{
			if (trueTypeFont != null || bitmapFont == null)
			{
				return 1f;
			}
			return (float)fontSize / (float)bitmapFont.defaultSize;
		}
	}

	public void Set(string value, bool notify = true)
	{
		if (!(mSelectedItem != value))
		{
			return;
		}
		mSelectedItem = value;
		if (mSelectedItem != null)
		{
			if (notify && mSelectedItem != null)
			{
				TriggerCallbacks();
			}
			mSelectedItem = null;
		}
	}

	public virtual void Clear()
	{
		items.Clear();
		itemData.Clear();
	}

	public virtual void AddItem(string text)
	{
		items.Add(text);
		itemData.Add(null);
	}

	public virtual void AddItem(string text, object data)
	{
		items.Add(text);
		itemData.Add(data);
	}

	public virtual void RemoveItem(string text)
	{
		int num = items.IndexOf(text);
		if (num != -1)
		{
			items.RemoveAt(num);
			itemData.RemoveAt(num);
		}
	}

	public virtual void RemoveItemByData(object data)
	{
		int num = itemData.IndexOf(data);
		if (num != -1)
		{
			items.RemoveAt(num);
			itemData.RemoveAt(num);
		}
	}

	protected void TriggerCallbacks()
	{
		if (!mExecuting)
		{
			mExecuting = true;
			UIPopupList uIPopupList = current;
			current = this;
			if (mLegacyEvent != null)
			{
				mLegacyEvent(mSelectedItem);
			}
			if (EventDelegate.IsValid(onChange))
			{
				EventDelegate.Execute(onChange);
			}
			else if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
			{
				eventReceiver.SendMessage(functionName, mSelectedItem, SendMessageOptions.DontRequireReceiver);
			}
			current = uIPopupList;
			mExecuting = false;
		}
	}

	protected virtual void OnEnable()
	{
		if (EventDelegate.IsValid(onChange))
		{
			eventReceiver = null;
			functionName = null;
		}
		if (font != null)
		{
			if (font.isDynamic)
			{
				trueTypeFont = font.dynamicFont;
				fontStyle = font.dynamicFontStyle;
				mUseDynamicFont = true;
			}
			else if (bitmapFont == null)
			{
				bitmapFont = font;
				mUseDynamicFont = false;
			}
			font = null;
		}
		if (textScale != 0f)
		{
			fontSize = ((!(bitmapFont != null)) ? 16 : Mathf.RoundToInt((float)bitmapFont.defaultSize * textScale));
			textScale = 0f;
		}
		if (trueTypeFont == null && bitmapFont != null && bitmapFont.isDynamic)
		{
			trueTypeFont = bitmapFont.dynamicFont;
			bitmapFont = null;
		}
	}

	protected virtual void OnValidate()
	{
		Font font = trueTypeFont;
		UIFont uIFont = bitmapFont;
		bitmapFont = null;
		trueTypeFont = null;
		if (font != null && (uIFont == null || !mUseDynamicFont))
		{
			bitmapFont = null;
			trueTypeFont = font;
			mUseDynamicFont = true;
		}
		else if (uIFont != null)
		{
			if (uIFont.isDynamic)
			{
				trueTypeFont = uIFont.dynamicFont;
				fontStyle = uIFont.dynamicFontStyle;
				fontSize = uIFont.defaultSize;
				mUseDynamicFont = true;
			}
			else
			{
				bitmapFont = uIFont;
				mUseDynamicFont = false;
			}
		}
		else
		{
			trueTypeFont = font;
			mUseDynamicFont = true;
		}
	}

	public virtual void Start()
	{
		if (!mStarted)
		{
			mStarted = true;
			if (textLabel != null)
			{
				EventDelegate.Add(onChange, textLabel.SetCurrentSelection);
				textLabel = null;
			}
		}
	}

	protected virtual void OnLocalize()
	{
		if (isLocalized)
		{
			TriggerCallbacks();
		}
	}

	protected virtual void Highlight(UILabel lbl, bool instant)
	{
		if (!(mHighlight != null))
		{
			return;
		}
		mHighlightedLabel = lbl;
		Vector3 highlightPosition = GetHighlightPosition();
		if (!instant && isAnimated)
		{
			TweenPosition.Begin(mHighlight.gameObject, 0.1f, highlightPosition).method = UITweener.Method.EaseOut;
			if (!mTweening)
			{
				mTweening = true;
				StartCoroutine("UpdateTweenPosition");
			}
		}
		else
		{
			mHighlight.cachedTransform.localPosition = highlightPosition;
		}
	}

	protected virtual Vector3 GetHighlightPosition()
	{
		if (mHighlightedLabel == null || mHighlight == null)
		{
			return Vector3.zero;
		}
		Vector4 border = mHighlight.border;
		float num = ((!(atlas != null)) ? 1f : atlas.pixelSize);
		float num2 = border.x * num;
		float y = border.w * num;
		return mHighlightedLabel.cachedTransform.localPosition + new Vector3(0f - num2, y, 1f);
	}

	protected virtual IEnumerator UpdateTweenPosition()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CUpdateTweenPosition_003Ed__87(0)
		{
			_003C_003E4__this = this
		};
	}

	protected virtual void OnItemHover(GameObject go, bool isOver)
	{
		if (isOver)
		{
			UILabel component = go.GetComponent<UILabel>();
			Highlight(component, instant: false);
		}
	}

	protected virtual void OnItemPress(GameObject go, bool isPressed)
	{
		if (!isPressed)
		{
			return;
		}
		Select(go.GetComponent<UILabel>(), instant: true);
		UIEventListener component = go.GetComponent<UIEventListener>();
		value = component.parameter as string;
		UIPlaySound[] components = GetComponents<UIPlaySound>();
		int i = 0;
		for (int num = components.Length; i < num; i++)
		{
			UIPlaySound uIPlaySound = components[i];
			if (uIPlaySound.trigger == UIPlaySound.Trigger.OnClick)
			{
				NGUITools.PlaySound(uIPlaySound.audioClip, uIPlaySound.volume, 1f);
			}
		}
		CloseSelf();
	}

	private void Select(UILabel lbl, bool instant)
	{
		Highlight(lbl, instant);
	}

	protected virtual void OnNavigate(KeyCode key)
	{
		if (!base.enabled || !(current == this))
		{
			return;
		}
		int num = mLabelList.IndexOf(mHighlightedLabel);
		if (num == -1)
		{
			num = 0;
		}
		switch (key)
		{
		case KeyCode.UpArrow:
			if (num > 0)
			{
				Select(mLabelList[--num], instant: false);
			}
			break;
		case KeyCode.DownArrow:
			if (num + 1 < mLabelList.Count)
			{
				Select(mLabelList[++num], instant: false);
			}
			break;
		}
	}

	protected virtual void OnKey(KeyCode key)
	{
		if (base.enabled && current == this && (key == UICamera.current.cancelKey0 || key == UICamera.current.cancelKey1))
		{
			OnSelect(isSelected: false);
		}
	}

	protected virtual void OnDisable()
	{
		CloseSelf();
	}

	protected virtual void OnSelect(bool isSelected)
	{
		if (!isSelected)
		{
			CloseSelf();
		}
	}

	public static void Close()
	{
		if (current != null)
		{
			current.CloseSelf();
			current = null;
		}
	}

	public virtual void CloseSelf()
	{
		if (!(mChild != null) || !(current == this))
		{
			return;
		}
		StopCoroutine("CloseIfUnselected");
		mSelection = null;
		mLabelList.Clear();
		if (isAnimated)
		{
			UIWidget[] componentsInChildren = mChild.GetComponentsInChildren<UIWidget>();
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				UIWidget obj = componentsInChildren[i];
				Color color = obj.color;
				color.a = 0f;
				TweenColor.Begin(obj.gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
			}
			Collider[] componentsInChildren2 = mChild.GetComponentsInChildren<Collider>();
			int j = 0;
			for (int num2 = componentsInChildren2.Length; j < num2; j++)
			{
				componentsInChildren2[j].enabled = false;
			}
			UnityEngine.Object.Destroy(mChild, 0.15f);
			mFadeOutComplete = Time.unscaledTime + Mathf.Max(0.1f, 0.15f);
		}
		else
		{
			UnityEngine.Object.Destroy(mChild);
			mFadeOutComplete = Time.unscaledTime + 0.1f;
		}
		mBackground = null;
		mHighlight = null;
		mChild = null;
		current = null;
	}

	protected virtual void AnimateColor(UIWidget widget)
	{
		Color color = widget.color;
		widget.color = new Color(color.r, color.g, color.b, 0f);
		TweenColor.Begin(widget.gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
	}

	protected virtual void AnimatePosition(UIWidget widget, bool placeAbove, float bottom)
	{
		Vector3 localPosition = widget.cachedTransform.localPosition;
		Vector3 localPosition2 = ((!placeAbove) ? new Vector3(localPosition.x, 0f, localPosition.z) : new Vector3(localPosition.x, bottom, localPosition.z));
		widget.cachedTransform.localPosition = localPosition2;
		TweenPosition.Begin(widget.gameObject, 0.15f, localPosition).method = UITweener.Method.EaseOut;
	}

	protected virtual void AnimateScale(UIWidget widget, bool placeAbove, float bottom)
	{
		GameObject go = widget.gameObject;
		Transform cachedTransform = widget.cachedTransform;
		float num = (float)activeFontSize * activeFontScale + mBgBorder * 2f;
		cachedTransform.localScale = new Vector3(1f, num / (float)widget.height, 1f);
		TweenScale.Begin(go, 0.15f, Vector3.one).method = UITweener.Method.EaseOut;
		if (placeAbove)
		{
			Vector3 localPosition = cachedTransform.localPosition;
			cachedTransform.localPosition = new Vector3(localPosition.x, localPosition.y - (float)widget.height + num, localPosition.z);
			TweenPosition.Begin(go, 0.15f, localPosition).method = UITweener.Method.EaseOut;
		}
	}

	private void Animate(UIWidget widget, bool placeAbove, float bottom)
	{
		AnimateColor(widget);
		AnimatePosition(widget, placeAbove, bottom);
	}

	protected virtual void OnClick()
	{
		if (mOpenFrame == Time.frameCount)
		{
			return;
		}
		if (mChild == null)
		{
			if (openOn != OpenOn.DoubleClick && openOn != OpenOn.Manual && (openOn != OpenOn.RightClick || UICamera.currentTouchID == -2))
			{
				Show();
			}
		}
		else if (mHighlightedLabel != null)
		{
			OnItemPress(mHighlightedLabel.gameObject, isPressed: true);
		}
	}

	protected virtual void OnDoubleClick()
	{
		if (openOn == OpenOn.DoubleClick)
		{
			Show();
		}
	}

	private IEnumerator CloseIfUnselected()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCloseIfUnselected_003Ed__103(0)
		{
			_003C_003E4__this = this
		};
	}

	public virtual void Show()
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject) && mChild == null && isValid && items.Count > 0)
		{
			mLabelList.Clear();
			StopCoroutine("CloseIfUnselected");
			UICamera.selectedObject = UICamera.hoveredObject ?? base.gameObject;
			mSelection = UICamera.selectedObject;
			source = UICamera.selectedObject;
			if (source == null)
			{
				Debug.LogError("Popup list needs a source object...");
				return;
			}
			mOpenFrame = Time.frameCount;
			if (mPanel == null)
			{
				mPanel = UIPanel.Find(base.transform);
				if (mPanel == null)
				{
					return;
				}
			}
			mChild = new GameObject("Drop-down List");
			mChild.layer = base.gameObject.layer;
			if (separatePanel)
			{
				if (GetComponent<Collider>() != null)
				{
					mChild.AddComponent<Rigidbody>().isKinematic = true;
				}
				else if (GetComponent<Collider2D>() != null)
				{
					mChild.AddComponent<Rigidbody2D>().isKinematic = true;
				}
				mChild.AddComponent<UIPanel>().depth = 1000000;
			}
			current = this;
			Transform transform = mChild.transform;
			transform.parent = mPanel.cachedTransform;
			Vector3 vector;
			Vector3 vector2;
			if (openOn == OpenOn.Manual && mSelection != base.gameObject)
			{
				startingPosition = UICamera.lastEventPosition;
				vector = mPanel.cachedTransform.InverseTransformPoint(mPanel.anchorCamera.ScreenToWorldPoint(startingPosition));
				vector2 = vector;
				transform.localPosition = vector;
				startingPosition = transform.position;
			}
			else
			{
				Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, base.transform, considerInactive: false, considerChildren: false);
				vector = bounds.min;
				vector2 = bounds.max;
				transform.localPosition = vector;
				startingPosition = transform.position;
			}
			StartCoroutine("CloseIfUnselected");
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			int num = ((!separatePanel) ? NGUITools.CalculateNextDepth(mPanel.gameObject) : 0);
			if (background2DSprite != null)
			{
				UI2DSprite uI2DSprite = mChild.AddWidget<UI2DSprite>(num);
				uI2DSprite.sprite2D = background2DSprite;
				mBackground = uI2DSprite;
			}
			else
			{
				if (!(atlas != null))
				{
					return;
				}
				mBackground = mChild.AddSprite(backgroundSprite, num);
			}
			mBackground.pivot = UIWidget.Pivot.TopLeft;
			mBackground.color = backgroundColor;
			Vector4 border = mBackground.border;
			mBgBorder = border.y;
			mBackground.cachedTransform.localPosition = new Vector3(0f, border.y, 0f);
			if (highlight2DSprite != null)
			{
				UI2DSprite uI2DSprite2 = mChild.AddWidget<UI2DSprite>(++num);
				uI2DSprite2.sprite2D = highlight2DSprite;
				mHighlight = uI2DSprite2;
			}
			else
			{
				if (!(atlas != null))
				{
					return;
				}
				mHighlight = mChild.AddSprite(highlightSprite, ++num);
			}
			float num2 = 0f;
			float num3 = 0f;
			if (mHighlight.hasBorder)
			{
				num2 = mHighlight.border.w;
				num3 = mHighlight.border.x;
			}
			mHighlight.pivot = UIWidget.Pivot.TopLeft;
			mHighlight.color = highlightColor;
			float num4 = activeFontSize;
			float num5 = activeFontScale;
			float num6 = num4 * num5;
			float a = 0f;
			float num7 = 0f - padding.y;
			List<UILabel> list = new List<UILabel>();
			if (!items.Contains(mSelectedItem))
			{
				mSelectedItem = null;
			}
			int i = 0;
			for (int count = items.Count; i < count; i++)
			{
				string text = items[i];
				UILabel uILabel = mChild.AddWidget<UILabel>(mBackground.depth + 2);
				uILabel.name = i.ToString();
				uILabel.pivot = UIWidget.Pivot.TopLeft;
				uILabel.bitmapFont = bitmapFont;
				uILabel.trueTypeFont = trueTypeFont;
				uILabel.fontSize = fontSize;
				uILabel.fontStyle = fontStyle;
				uILabel.text = ((!isLocalized) ? text : Localization.Get(text));
				uILabel.color = textColor;
				uILabel.cachedTransform.localPosition = new Vector3(border.x + padding.x - uILabel.pivotOffset.x, num7, -1f);
				uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
				uILabel.alignment = alignment;
				list.Add(uILabel);
				num7 -= num6;
				num7 -= padding.y;
				a = Mathf.Max(a, uILabel.printedSize.x);
				UIEventListener uIEventListener = UIEventListener.Get(uILabel.gameObject);
				uIEventListener.onHover = OnItemHover;
				uIEventListener.onPress = OnItemPress;
				uIEventListener.parameter = text;
				if (mSelectedItem == text || (i == 0 && string.IsNullOrEmpty(mSelectedItem)))
				{
					Highlight(uILabel, instant: true);
				}
				mLabelList.Add(uILabel);
			}
			a = Mathf.Max(a, vector2.x - vector.x - (border.x + padding.x) * 2f);
			float num8 = a;
			Vector3 vector3 = new Vector3(num8 * 0.5f, (0f - num6) * 0.5f, 0f);
			Vector3 vector4 = new Vector3(num8, num6 + padding.y, 1f);
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				UILabel uILabel2 = list[j];
				NGUITools.AddWidgetCollider(uILabel2.gameObject);
				uILabel2.autoResizeBoxCollider = false;
				BoxCollider component = uILabel2.GetComponent<BoxCollider>();
				if (component != null)
				{
					vector3.z = component.center.z;
					component.center = vector3;
					component.size = vector4;
				}
				else
				{
					BoxCollider2D component2 = uILabel2.GetComponent<BoxCollider2D>();
					component2.offset = vector3;
					component2.size = vector4;
				}
			}
			int width = Mathf.RoundToInt(a);
			a += (border.x + padding.x) * 2f;
			num7 -= border.y;
			mBackground.width = Mathf.RoundToInt(a);
			mBackground.height = Mathf.RoundToInt(0f - num7 + border.y);
			int k = 0;
			for (int count3 = list.Count; k < count3; k++)
			{
				UILabel uILabel3 = list[k];
				uILabel3.overflowMethod = UILabel.Overflow.ShrinkContent;
				uILabel3.width = width;
			}
			float num9 = ((!(atlas != null)) ? 2f : (2f * atlas.pixelSize));
			float f = a - (border.x + padding.x) * 2f + num3 * num9;
			float f2 = num6 + num2 * num9;
			mHighlight.width = Mathf.RoundToInt(f);
			mHighlight.height = Mathf.RoundToInt(f2);
			bool flag = position == Position.Above;
			if (position == Position.Auto)
			{
				UICamera uICamera = UICamera.FindCameraForLayer(mSelection.layer);
				if (uICamera != null)
				{
					flag = uICamera.cachedCamera.WorldToViewportPoint(startingPosition).y < 0.5f;
				}
			}
			if (isAnimated)
			{
				AnimateColor(mBackground);
				if (Time.timeScale == 0f || Time.timeScale >= 0.1f)
				{
					float bottom = num7 + num6;
					Animate(mHighlight, flag, bottom);
					int l = 0;
					for (int count4 = list.Count; l < count4; l++)
					{
						Animate(list[l], flag, bottom);
					}
					AnimateScale(mBackground, flag, bottom);
				}
			}
			if (flag)
			{
				vector.y = vector2.y - border.y;
				vector2.y = vector.y + (float)mBackground.height;
				vector2.x = vector.x + (float)mBackground.width;
				transform.localPosition = new Vector3(vector.x, vector2.y - border.y, vector.z);
			}
			else
			{
				vector2.y = vector.y + border.y;
				vector.y = vector2.y - (float)mBackground.height;
				vector2.x = vector.x + (float)mBackground.width;
			}
			Transform parent = mPanel.cachedTransform.parent;
			if (parent != null)
			{
				vector = mPanel.cachedTransform.TransformPoint(vector);
				vector2 = mPanel.cachedTransform.TransformPoint(vector2);
				vector = parent.InverseTransformPoint(vector);
				vector2 = parent.InverseTransformPoint(vector2);
			}
			Vector3 vector5 = ((!mPanel.hasClipping) ? mPanel.CalculateConstrainOffset(vector, vector2) : Vector3.zero);
			Vector3 localPosition = transform.localPosition + vector5;
			localPosition.x = Mathf.Round(localPosition.x);
			localPosition.y = Mathf.Round(localPosition.y);
			transform.localPosition = localPosition;
		}
		else
		{
			OnSelect(isSelected: false);
		}
	}
}
