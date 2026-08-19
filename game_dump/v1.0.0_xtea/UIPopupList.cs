using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Popup List")]
[ExecuteInEditMode]
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

	private const float animSpeed = 0.15f;

	public static UIPopupList current;

	private static GameObject mChild;

	private static float mFadeOutComplete;

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

	public Vector2 padding = Vector2.op_Implicit(new Vector3(4f, 4f));

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

	[SerializeField]
	[HideInInspector]
	protected UIBasicSprite mBackground;

	[SerializeField]
	[HideInInspector]
	protected UIBasicSprite mHighlight;

	[HideInInspector]
	[SerializeField]
	protected UILabel mHighlightedLabel;

	[SerializeField]
	[HideInInspector]
	protected List<UILabel> mLabelList = new List<UILabel>();

	[SerializeField]
	[HideInInspector]
	protected float mBgBorder;

	[NonSerialized]
	protected GameObject mSelection;

	[NonSerialized]
	protected int mOpenFrame;

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[SerializeField]
	[HideInInspector]
	private string functionName = "OnSelectionChange";

	[HideInInspector]
	[SerializeField]
	private float textScale;

	[HideInInspector]
	[SerializeField]
	private UIFont font;

	[SerializeField]
	[HideInInspector]
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

	public Object ambigiousFont
	{
		get
		{
			if ((Object)(object)trueTypeFont != (Object)null)
			{
				return (Object)(object)trueTypeFont;
			}
			if ((Object)(object)bitmapFont != (Object)null)
			{
				return (Object)(object)bitmapFont;
			}
			return (Object)(object)font;
		}
		set
		{
			if (value is Font)
			{
				trueTypeFont = (Font)(object)((value is Font) ? value : null);
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

	public static bool isOpen => (Object)(object)current != (Object)null && ((Object)(object)mChild != (Object)null || mFadeOutComplete > Time.unscaledTime);

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
			return (num <= -1 || num >= itemData.Count) ? null : itemData[num];
		}
	}

	public bool isColliderEnabled
	{
		get
		{
			Collider component = ((Component)this).GetComponent<Collider>();
			if ((Object)(object)component != (Object)null)
			{
				return component.enabled;
			}
			Collider2D component2 = ((Component)this).GetComponent<Collider2D>();
			return (Object)(object)component2 != (Object)null && ((Behaviour)component2).enabled;
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

	private bool isValid => (Object)(object)bitmapFont != (Object)null || (Object)(object)trueTypeFont != (Object)null;

	private int activeFontSize => (!((Object)(object)trueTypeFont != (Object)null) && !((Object)(object)bitmapFont == (Object)null)) ? bitmapFont.defaultSize : fontSize;

	private float activeFontScale => (!((Object)(object)trueTypeFont != (Object)null) && !((Object)(object)bitmapFont == (Object)null)) ? ((float)fontSize / (float)bitmapFont.defaultSize) : 1f;

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
			else if ((Object)(object)eventReceiver != (Object)null && !string.IsNullOrEmpty(functionName))
			{
				eventReceiver.SendMessage(functionName, (object)mSelectedItem, (SendMessageOptions)1);
			}
			current = uIPopupList;
			mExecuting = false;
		}
	}

	protected virtual void OnEnable()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (EventDelegate.IsValid(onChange))
		{
			eventReceiver = null;
			functionName = null;
		}
		if ((Object)(object)font != (Object)null)
		{
			if (font.isDynamic)
			{
				trueTypeFont = font.dynamicFont;
				fontStyle = font.dynamicFontStyle;
				mUseDynamicFont = true;
			}
			else if ((Object)(object)bitmapFont == (Object)null)
			{
				bitmapFont = font;
				mUseDynamicFont = false;
			}
			font = null;
		}
		if (textScale != 0f)
		{
			fontSize = ((!((Object)(object)bitmapFont != (Object)null)) ? 16 : Mathf.RoundToInt((float)bitmapFont.defaultSize * textScale));
			textScale = 0f;
		}
		if ((Object)(object)trueTypeFont == (Object)null && (Object)(object)bitmapFont != (Object)null && bitmapFont.isDynamic)
		{
			trueTypeFont = bitmapFont.dynamicFont;
			bitmapFont = null;
		}
	}

	protected virtual void OnValidate()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		Font val = trueTypeFont;
		UIFont uIFont = bitmapFont;
		bitmapFont = null;
		trueTypeFont = null;
		if ((Object)(object)val != (Object)null && ((Object)(object)uIFont == (Object)null || !mUseDynamicFont))
		{
			bitmapFont = null;
			trueTypeFont = val;
			mUseDynamicFont = true;
		}
		else if ((Object)(object)uIFont != (Object)null)
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
			trueTypeFont = val;
			mUseDynamicFont = true;
		}
	}

	public virtual void Start()
	{
		if (!mStarted)
		{
			mStarted = true;
			if ((Object)(object)textLabel != (Object)null)
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
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)mHighlight != (Object)null))
		{
			return;
		}
		mHighlightedLabel = lbl;
		Vector3 highlightPosition = GetHighlightPosition();
		if (!instant && isAnimated)
		{
			TweenPosition.Begin(((Component)mHighlight).gameObject, 0.1f, highlightPosition).method = UITweener.Method.EaseOut;
			if (!mTweening)
			{
				mTweening = true;
				((MonoBehaviour)this).StartCoroutine("UpdateTweenPosition");
			}
		}
		else
		{
			mHighlight.cachedTransform.localPosition = highlightPosition;
		}
	}

	protected virtual Vector3 GetHighlightPosition()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mHighlightedLabel == (Object)null || (Object)(object)mHighlight == (Object)null)
		{
			return Vector3.zero;
		}
		Vector4 border = mHighlight.border;
		float num = ((!((Object)(object)atlas != (Object)null)) ? 1f : atlas.pixelSize);
		float num2 = border.x * num;
		float num3 = border.w * num;
		return mHighlightedLabel.cachedTransform.localPosition + new Vector3(0f - num2, num3, 1f);
	}

	protected virtual IEnumerator UpdateTweenPosition()
	{
		if ((Object)(object)mHighlight != (Object)null && (Object)(object)mHighlightedLabel != (Object)null)
		{
			TweenPosition tp = ((Component)mHighlight).GetComponent<TweenPosition>();
			while ((Object)(object)tp != (Object)null && ((Behaviour)tp).enabled)
			{
				tp.to = GetHighlightPosition();
				yield return null;
			}
		}
		mTweening = false;
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
		UIPlaySound[] components = ((Component)this).GetComponents<UIPlaySound>();
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
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		if (!((Behaviour)this).enabled || !((Object)(object)current == (Object)(object)this))
		{
			return;
		}
		int num = mLabelList.IndexOf(mHighlightedLabel);
		if (num == -1)
		{
			num = 0;
		}
		if ((int)key == 273)
		{
			if (num > 0)
			{
				Select(mLabelList[--num], instant: false);
			}
		}
		else if ((int)key == 274 && num + 1 < mLabelList.Count)
		{
			Select(mLabelList[++num], instant: false);
		}
	}

	protected virtual void OnKey(KeyCode key)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled && (Object)(object)current == (Object)(object)this && (key == UICamera.current.cancelKey0 || key == UICamera.current.cancelKey1))
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
		if ((Object)(object)current != (Object)null)
		{
			current.CloseSelf();
			current = null;
		}
	}

	public virtual void CloseSelf()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)mChild != (Object)null) || !((Object)(object)current == (Object)(object)this))
		{
			return;
		}
		((MonoBehaviour)this).StopCoroutine("CloseIfUnselected");
		mSelection = null;
		mLabelList.Clear();
		if (isAnimated)
		{
			UIWidget[] componentsInChildren = mChild.GetComponentsInChildren<UIWidget>();
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				UIWidget uIWidget = componentsInChildren[i];
				Color color = uIWidget.color;
				color.a = 0f;
				TweenColor.Begin(((Component)uIWidget).gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
			}
			Collider[] componentsInChildren2 = mChild.GetComponentsInChildren<Collider>();
			int j = 0;
			for (int num2 = componentsInChildren2.Length; j < num2; j++)
			{
				componentsInChildren2[j].enabled = false;
			}
			Object.Destroy((Object)(object)mChild, 0.15f);
			mFadeOutComplete = Time.unscaledTime + Mathf.Max(0.1f, 0.15f);
		}
		else
		{
			Object.Destroy((Object)(object)mChild);
			mFadeOutComplete = Time.unscaledTime + 0.1f;
		}
		mBackground = null;
		mHighlight = null;
		mChild = null;
		current = null;
	}

	protected virtual void AnimateColor(UIWidget widget)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Color color = widget.color;
		widget.color = new Color(color.r, color.g, color.b, 0f);
		TweenColor.Begin(((Component)widget).gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
	}

	protected virtual void AnimatePosition(UIWidget widget, bool placeAbove, float bottom)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = widget.cachedTransform.localPosition;
		Vector3 localPosition2 = ((!placeAbove) ? new Vector3(localPosition.x, 0f, localPosition.z) : new Vector3(localPosition.x, bottom, localPosition.z));
		widget.cachedTransform.localPosition = localPosition2;
		GameObject gameObject = ((Component)widget).gameObject;
		TweenPosition.Begin(gameObject, 0.15f, localPosition).method = UITweener.Method.EaseOut;
	}

	protected virtual void AnimateScale(UIWidget widget, bool placeAbove, float bottom)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = ((Component)widget).gameObject;
		Transform cachedTransform = widget.cachedTransform;
		float num = (float)activeFontSize * activeFontScale + mBgBorder * 2f;
		cachedTransform.localScale = new Vector3(1f, num / (float)widget.height, 1f);
		TweenScale.Begin(gameObject, 0.15f, Vector3.one).method = UITweener.Method.EaseOut;
		if (placeAbove)
		{
			Vector3 localPosition = cachedTransform.localPosition;
			cachedTransform.localPosition = new Vector3(localPosition.x, localPosition.y - (float)widget.height + num, localPosition.z);
			TweenPosition.Begin(gameObject, 0.15f, localPosition).method = UITweener.Method.EaseOut;
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
		if ((Object)(object)mChild == (Object)null)
		{
			if (openOn != OpenOn.DoubleClick && openOn != OpenOn.Manual && (openOn != OpenOn.RightClick || UICamera.currentTouchID == -2))
			{
				Show();
			}
		}
		else if ((Object)(object)mHighlightedLabel != (Object)null)
		{
			OnItemPress(((Component)mHighlightedLabel).gameObject, isPressed: true);
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
		do
		{
			yield return null;
		}
		while (!((Object)(object)UICamera.selectedObject != (Object)(object)mSelection));
		CloseSelf();
	}

	public virtual void Show()
	{
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_0573: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0746: Unknown result type (might be due to invalid IL or missing references)
		//IL_0748: Unknown result type (might be due to invalid IL or missing references)
		//IL_0754: Unknown result type (might be due to invalid IL or missing references)
		//IL_0756: Unknown result type (might be due to invalid IL or missing references)
		//IL_0711: Unknown result type (might be due to invalid IL or missing references)
		//IL_0716: Unknown result type (might be due to invalid IL or missing references)
		//IL_0726: Unknown result type (might be due to invalid IL or missing references)
		//IL_072f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a70: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a82: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a85: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a86: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a94: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0acf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b00: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled && NGUITools.GetActive(((Component)this).gameObject) && (Object)(object)mChild == (Object)null && isValid && items.Count > 0)
		{
			mLabelList.Clear();
			((MonoBehaviour)this).StopCoroutine("CloseIfUnselected");
			UICamera.selectedObject = UICamera.hoveredObject ?? ((Component)this).gameObject;
			mSelection = UICamera.selectedObject;
			source = UICamera.selectedObject;
			if ((Object)(object)source == (Object)null)
			{
				Debug.LogError((object)"Popup list needs a source object...");
				return;
			}
			mOpenFrame = Time.frameCount;
			if ((Object)(object)mPanel == (Object)null)
			{
				mPanel = UIPanel.Find(((Component)this).transform);
				if ((Object)(object)mPanel == (Object)null)
				{
					return;
				}
			}
			mChild = new GameObject("Drop-down List");
			mChild.layer = ((Component)this).gameObject.layer;
			if (separatePanel)
			{
				if ((Object)(object)((Component)this).GetComponent<Collider>() != (Object)null)
				{
					Rigidbody val = mChild.AddComponent<Rigidbody>();
					val.isKinematic = true;
				}
				else if ((Object)(object)((Component)this).GetComponent<Collider2D>() != (Object)null)
				{
					Rigidbody2D val2 = mChild.AddComponent<Rigidbody2D>();
					val2.isKinematic = true;
				}
				mChild.AddComponent<UIPanel>().depth = 1000000;
			}
			current = this;
			Transform transform = mChild.transform;
			transform.parent = mPanel.cachedTransform;
			Vector3 val3;
			Vector3 val4;
			if (openOn == OpenOn.Manual && (Object)(object)mSelection != (Object)(object)((Component)this).gameObject)
			{
				startingPosition = Vector2.op_Implicit(UICamera.lastEventPosition);
				val3 = mPanel.cachedTransform.InverseTransformPoint(mPanel.anchorCamera.ScreenToWorldPoint(startingPosition));
				val4 = val3;
				transform.localPosition = val3;
				startingPosition = transform.position;
			}
			else
			{
				Bounds val5 = NGUIMath.CalculateRelativeWidgetBounds(mPanel.cachedTransform, ((Component)this).transform, considerInactive: false, considerChildren: false);
				val3 = ((Bounds)(ref val5)).min;
				val4 = ((Bounds)(ref val5)).max;
				transform.localPosition = val3;
				startingPosition = transform.position;
			}
			((MonoBehaviour)this).StartCoroutine("CloseIfUnselected");
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			int num = ((!separatePanel) ? NGUITools.CalculateNextDepth(((Component)mPanel).gameObject) : 0);
			if ((Object)(object)background2DSprite != (Object)null)
			{
				UI2DSprite uI2DSprite = mChild.AddWidget<UI2DSprite>(num);
				uI2DSprite.sprite2D = background2DSprite;
				mBackground = uI2DSprite;
			}
			else
			{
				if (!((Object)(object)atlas != (Object)null))
				{
					return;
				}
				mBackground = mChild.AddSprite(atlas, backgroundSprite, num);
			}
			mBackground.pivot = UIWidget.Pivot.TopLeft;
			mBackground.color = backgroundColor;
			Vector4 border = mBackground.border;
			mBgBorder = border.y;
			mBackground.cachedTransform.localPosition = new Vector3(0f, border.y, 0f);
			if ((Object)(object)highlight2DSprite != (Object)null)
			{
				UI2DSprite uI2DSprite2 = mChild.AddWidget<UI2DSprite>(++num);
				uI2DSprite2.sprite2D = highlight2DSprite;
				mHighlight = uI2DSprite2;
			}
			else
			{
				if (!((Object)(object)atlas != (Object)null))
				{
					return;
				}
				mHighlight = mChild.AddSprite(atlas, highlightSprite, ++num);
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
			float num7 = 0f;
			float num8 = 0f - padding.y;
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
				((Object)uILabel).name = i.ToString();
				uILabel.pivot = UIWidget.Pivot.TopLeft;
				uILabel.bitmapFont = bitmapFont;
				uILabel.trueTypeFont = trueTypeFont;
				uILabel.fontSize = fontSize;
				uILabel.fontStyle = fontStyle;
				uILabel.text = ((!isLocalized) ? text : Localization.Get(text));
				uILabel.color = textColor;
				uILabel.cachedTransform.localPosition = new Vector3(border.x + padding.x - uILabel.pivotOffset.x, num8, -1f);
				uILabel.overflowMethod = UILabel.Overflow.ResizeFreely;
				uILabel.alignment = alignment;
				list.Add(uILabel);
				num8 -= num6;
				num8 -= padding.y;
				num7 = Mathf.Max(num7, uILabel.printedSize.x);
				UIEventListener uIEventListener = UIEventListener.Get(((Component)uILabel).gameObject);
				uIEventListener.onHover = OnItemHover;
				uIEventListener.onPress = OnItemPress;
				uIEventListener.parameter = text;
				if (mSelectedItem == text || (i == 0 && string.IsNullOrEmpty(mSelectedItem)))
				{
					Highlight(uILabel, instant: true);
				}
				mLabelList.Add(uILabel);
			}
			num7 = Mathf.Max(num7, val4.x - val3.x - (border.x + padding.x) * 2f);
			float num9 = num7;
			Vector3 val6 = default(Vector3);
			((Vector3)(ref val6))._002Ector(num9 * 0.5f, (0f - num6) * 0.5f, 0f);
			Vector3 val7 = default(Vector3);
			((Vector3)(ref val7))._002Ector(num9, num6 + padding.y, 1f);
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				UILabel uILabel2 = list[j];
				NGUITools.AddWidgetCollider(((Component)uILabel2).gameObject);
				uILabel2.autoResizeBoxCollider = false;
				BoxCollider component = ((Component)uILabel2).GetComponent<BoxCollider>();
				if ((Object)(object)component != (Object)null)
				{
					val6.z = component.center.z;
					component.center = val6;
					component.size = val7;
				}
				else
				{
					BoxCollider2D component2 = ((Component)uILabel2).GetComponent<BoxCollider2D>();
					((Collider2D)component2).offset = Vector2.op_Implicit(val6);
					component2.size = Vector2.op_Implicit(val7);
				}
			}
			int width = Mathf.RoundToInt(num7);
			num7 += (border.x + padding.x) * 2f;
			num8 -= border.y;
			mBackground.width = Mathf.RoundToInt(num7);
			mBackground.height = Mathf.RoundToInt(0f - num8 + border.y);
			int k = 0;
			for (int count3 = list.Count; k < count3; k++)
			{
				UILabel uILabel3 = list[k];
				uILabel3.overflowMethod = UILabel.Overflow.ShrinkContent;
				uILabel3.width = width;
			}
			float num10 = ((!((Object)(object)atlas != (Object)null)) ? 2f : (2f * atlas.pixelSize));
			float num11 = num7 - (border.x + padding.x) * 2f + num3 * num10;
			float num12 = num6 + num2 * num10;
			mHighlight.width = Mathf.RoundToInt(num11);
			mHighlight.height = Mathf.RoundToInt(num12);
			bool flag = position == Position.Above;
			if (position == Position.Auto)
			{
				UICamera uICamera = UICamera.FindCameraForLayer(mSelection.layer);
				if ((Object)(object)uICamera != (Object)null)
				{
					flag = uICamera.cachedCamera.WorldToViewportPoint(startingPosition).y < 0.5f;
				}
			}
			if (isAnimated)
			{
				AnimateColor(mBackground);
				if (Time.timeScale == 0f || Time.timeScale >= 0.1f)
				{
					float bottom = num8 + num6;
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
				val3.y = val4.y - border.y;
				val4.y = val3.y + (float)mBackground.height;
				val4.x = val3.x + (float)mBackground.width;
				transform.localPosition = new Vector3(val3.x, val4.y - border.y, val3.z);
			}
			else
			{
				val4.y = val3.y + border.y;
				val3.y = val4.y - (float)mBackground.height;
				val4.x = val3.x + (float)mBackground.width;
			}
			Transform parent = mPanel.cachedTransform.parent;
			if ((Object)(object)parent != (Object)null)
			{
				val3 = mPanel.cachedTransform.TransformPoint(val3);
				val4 = mPanel.cachedTransform.TransformPoint(val4);
				val3 = parent.InverseTransformPoint(val3);
				val4 = parent.InverseTransformPoint(val4);
			}
			Vector3 val8 = ((!mPanel.hasClipping) ? mPanel.CalculateConstrainOffset(Vector2.op_Implicit(val3), Vector2.op_Implicit(val4)) : Vector3.zero);
			Vector3 localPosition = transform.localPosition + val8;
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
