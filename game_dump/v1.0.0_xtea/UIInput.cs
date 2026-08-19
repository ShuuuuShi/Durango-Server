using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AndroidKeyboard;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Input Field")]
public class UIInput : MonoBehaviour
{
	public enum InputType
	{
		Standard,
		AutoCorrect,
		Password
	}

	public enum Validation
	{
		None,
		Integer,
		Float,
		Alphanumeric,
		Username,
		Name,
		Filename
	}

	public enum KeyboardType
	{
		Default,
		ASCIICapable,
		NumbersAndPunctuation,
		URL,
		NumberPad,
		PhonePad,
		NamePhonePad,
		EmailAddress
	}

	public enum OnReturnKey
	{
		Default,
		Submit,
		NewLine
	}

	public delegate char OnValidate(string text, int charIndex, char addedChar);

	public static UIInput current;

	public static UIInput selection;

	public UILabel label;

	public InputType inputType;

	public OnReturnKey onReturnKey;

	public KeyboardType keyboardType;

	public bool hideInput;

	public bool keepKeyboardOn;

	[NonSerialized]
	public bool selectAllTextOnFocus = true;

	public Validation validation;

	public int characterLimit;

	public string savedAs;

	[HideInInspector]
	[SerializeField]
	private GameObject selectOnTab;

	public Color activeTextColor = Color.white;

	public Color caretColor = new Color(1f, 1f, 1f, 0.8f);

	public Color selectionColor = new Color(1f, 0.8745098f, 47f / 85f, 0.5f);

	public List<EventDelegate> onSubmit = new List<EventDelegate>();

	public List<EventDelegate> onChange = new List<EventDelegate>();

	public OnValidate onValidate;

	[HideInInspector]
	[SerializeField]
	protected string mValue;

	[NonSerialized]
	protected string mDefaultText = string.Empty;

	[NonSerialized]
	protected Color mDefaultColor = Color.white;

	[NonSerialized]
	protected float mPosition;

	[NonSerialized]
	protected bool mDoInit = true;

	[NonSerialized]
	protected NGUIText.Alignment mAlignment = NGUIText.Alignment.Left;

	[NonSerialized]
	protected bool mLoadSavedValue = true;

	protected static int mDrawStart;

	protected static string mLastIME = string.Empty;

	protected static TouchScreenKeyboard mKeyboard;

	private static bool mWaitForKeyboard;

	[NonSerialized]
	protected int mSelectionStart;

	[NonSerialized]
	protected int mSelectionEnd;

	[NonSerialized]
	protected UITexture mHighlight;

	[NonSerialized]
	protected UITexture mCaret;

	[NonSerialized]
	protected Texture2D mBlankTex;

	[NonSerialized]
	protected float mNextBlink;

	[NonSerialized]
	protected float mLastAlpha;

	[NonSerialized]
	protected string mCached = string.Empty;

	[NonSerialized]
	protected int mSelectMe = -1;

	[NonSerialized]
	protected int mSelectTime = -1;

	[NonSerialized]
	protected bool mStarted;

	[NonSerialized]
	private UICamera mCam;

	private static int selectedUIInputID;

	[NonSerialized]
	private bool mEllipsis;

	private static int mIgnoreKey;

	private string compsitionstring = string.Empty;

	public string defaultText
	{
		get
		{
			if (mDoInit)
			{
				Init();
			}
			return mDefaultText;
		}
		set
		{
			if (mDoInit)
			{
				Init();
			}
			mDefaultText = value;
			UpdateLabel();
		}
	}

	public Color defaultColor
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if (mDoInit)
			{
				Init();
			}
			return mDefaultColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			mDefaultColor = value;
			if (!isSelected)
			{
				label.color = value;
			}
		}
	}

	public bool inputShouldBeHidden => hideInput && (Object)(object)label != (Object)null && !label.multiLine && inputType != InputType.Password;

	[Obsolete("Use UIInput.value instead")]
	public string text
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

	public string value
	{
		get
		{
			if (mDoInit)
			{
				Init();
			}
			return mValue;
		}
		set
		{
			Set(value);
		}
	}

	[Obsolete("Use UIInput.isSelected instead")]
	public bool selected
	{
		get
		{
			return isSelected;
		}
		set
		{
			isSelected = value;
		}
	}

	public bool isSelected
	{
		get
		{
			return (Object)(object)selection == (Object)(object)this;
		}
		set
		{
			if (TouchScreenKeyboard.instance != null && TouchScreenKeyboard.instance.OnReturnKey && !value)
			{
				return;
			}
			if (!value)
			{
				if (isSelected)
				{
					UICamera.selectedObject = null;
				}
			}
			else
			{
				UICamera.selectedObject = ((Component)this).gameObject;
			}
		}
	}

	public int cursorPosition
	{
		get
		{
			if (mKeyboard != null)
			{
				if (inputShouldBeHidden)
				{
					if (isSelected)
					{
						return mSelectionEnd;
					}
					return value.Length;
				}
				return value.Length;
			}
			return (!isSelected) ? value.Length : mSelectionEnd;
		}
		set
		{
			if (isSelected && (mKeyboard == null || inputShouldBeHidden))
			{
				mSelectionEnd = value;
				UpdateLabel();
			}
		}
	}

	public int selectionStart
	{
		get
		{
			if (mKeyboard != null && !inputShouldBeHidden)
			{
				return 0;
			}
			return (!isSelected) ? value.Length : mSelectionStart;
		}
		set
		{
			if (isSelected && (mKeyboard == null || inputShouldBeHidden))
			{
				if (!string.IsNullOrEmpty(Input.compositionString))
				{
					string compositionString = Input.compositionString;
					Input.compositionString = string.Empty;
					Insert(compositionString);
					AndroidKeyboardManager.ClearComposition();
				}
				mSelectionStart = value;
				UpdateLabel();
			}
		}
	}

	public int selectionEnd
	{
		get
		{
			if (mKeyboard != null && !inputShouldBeHidden)
			{
				return value.Length;
			}
			return (!isSelected) ? value.Length : mSelectionEnd;
		}
		set
		{
			if (isSelected && (mKeyboard == null || inputShouldBeHidden))
			{
				mSelectionEnd = value;
				UpdateLabel();
			}
		}
	}

	public UITexture caret => mCaret;

	private string Compsitionstring
	{
		get
		{
			return compsitionstring;
		}
		set
		{
			if (compsitionstring != value)
			{
				compsitionstring = value;
				DeleteSelection();
			}
		}
	}

	public void Set(string value, bool notify = true)
	{
		if (mDoInit)
		{
			Init();
		}
		if (value == this.value)
		{
			return;
		}
		mDrawStart = 0;
		value = Validate(value);
		if (isSelected && mKeyboard != null && mCached != value)
		{
			mKeyboard.text = value;
			mCached = value;
		}
		if (!(mValue != value))
		{
			return;
		}
		mValue = value;
		mLoadSavedValue = false;
		if (isSelected)
		{
			if (string.IsNullOrEmpty(value))
			{
				mSelectionStart = 0;
				mSelectionEnd = 0;
			}
			else
			{
				mSelectionStart = value.Length;
				mSelectionEnd = mSelectionStart;
			}
		}
		else if (mStarted)
		{
			SaveToPlayerPrefs(value);
		}
		UpdateLabel();
		if (notify)
		{
			ExecuteOnChange();
		}
	}

	public string Validate(string val)
	{
		if (string.IsNullOrEmpty(val))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(val.Length);
		for (int i = 0; i < val.Length; i++)
		{
			char c = val[i];
			if (onValidate != null)
			{
				c = onValidate(stringBuilder.ToString(), stringBuilder.Length, c);
			}
			else if (validation != 0)
			{
				c = Validate(stringBuilder.ToString(), stringBuilder.Length, c);
			}
			if (c != 0)
			{
				stringBuilder.Append(c);
			}
		}
		if (characterLimit > 0 && stringBuilder.Length > characterLimit)
		{
			return stringBuilder.ToString(0, characterLimit);
		}
		return stringBuilder.ToString();
	}

	public void Start()
	{
		if (mStarted)
		{
			return;
		}
		if ((Object)(object)selectOnTab != (Object)null)
		{
			UIKeyNavigation component = ((Component)this).GetComponent<UIKeyNavigation>();
			if ((Object)(object)component == (Object)null)
			{
				component = ((Component)this).gameObject.AddComponent<UIKeyNavigation>();
				component.onDown = selectOnTab;
			}
			selectOnTab = null;
			NGUITools.SetDirty((Object)(object)this);
		}
		if (mLoadSavedValue && !string.IsNullOrEmpty(savedAs))
		{
			LoadValue();
		}
		else
		{
			value = mValue.Replace("\\n", "\n");
		}
		mStarted = true;
	}

	protected void Init()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (mDoInit && (Object)(object)label != (Object)null)
		{
			mDoInit = false;
			mDefaultText = label.text;
			mDefaultColor = label.color;
			label.supportEncoding = false;
			mEllipsis = label.overflowEllipsis;
			if (label.alignment == NGUIText.Alignment.Justified)
			{
				label.alignment = NGUIText.Alignment.Left;
			}
			mAlignment = label.alignment;
			mPosition = label.cachedTransform.localPosition.x;
			UpdateLabel();
		}
	}

	protected void SaveToPlayerPrefs(string val)
	{
		if (!string.IsNullOrEmpty(savedAs))
		{
			if (string.IsNullOrEmpty(val))
			{
				PlayerPrefs.DeleteKey(savedAs);
			}
			else
			{
				PlayerPrefs.SetString(savedAs, val);
			}
		}
	}

	protected virtual void OnSelect(bool isSelected)
	{
		if (isSelected)
		{
			OnSelectEvent();
		}
		else
		{
			OnDeselectEvent();
		}
	}

	protected void OnSelectEvent()
	{
		mSelectTime = Time.frameCount;
		selection = this;
		selectedUIInputID = ((Object)this).GetInstanceID();
		OnSelectEvent_Android();
		if (mDoInit)
		{
			Init();
		}
		if ((Object)(object)label != (Object)null)
		{
			mEllipsis = label.overflowEllipsis;
			label.overflowEllipsis = false;
		}
		if ((Object)(object)label != (Object)null && NGUITools.GetActive((Behaviour)(object)this))
		{
			mSelectMe = Time.frameCount;
		}
	}

	protected void OnDeselectEvent()
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (mDoInit)
		{
			Init();
		}
		if ((Object)(object)label != (Object)null)
		{
			label.overflowEllipsis = mEllipsis;
		}
		if ((Object)(object)label != (Object)null && NGUITools.GetActive((Behaviour)(object)this))
		{
			mValue = value;
			if (mKeyboard != null)
			{
				if (!string.IsNullOrEmpty(Input.compositionString))
				{
					Insert(Input.compositionString);
				}
				Input.compositionString = string.Empty;
				if (!AdditionalOptions.keepKeyboardOn)
				{
					mWaitForKeyboard = false;
					mKeyboard.active = false;
					mKeyboard = null;
				}
			}
			if (string.IsNullOrEmpty(mValue))
			{
				label.text = mDefaultText;
				label.color = mDefaultColor;
			}
			else
			{
				label.text = mValue;
			}
			Input.imeCompositionMode = (IMECompositionMode)0;
			label.alignment = mAlignment;
		}
		selection = null;
		UpdateLabel();
	}

	protected virtual void Update()
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Invalid comparison between Unknown and I4
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Invalid comparison between Unknown and I4
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Invalid comparison between Unknown and I4
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Invalid comparison between Unknown and I4
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Invalid comparison between Unknown and I4
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		if (!isSelected || mSelectTime == Time.frameCount || ((mKeyboard == null || !mKeyboard.active || !AdditionalOptions.keepKeyboardOn) && !isSelected) || mSelectTime == Time.frameCount || selectedUIInputID != ((Object)this).GetInstanceID())
		{
			return;
		}
		if (mDoInit)
		{
			Init();
		}
		if (mWaitForKeyboard)
		{
			if (mKeyboard != null && !mKeyboard.active)
			{
				return;
			}
			mWaitForKeyboard = false;
		}
		if (mSelectMe != -1 && mSelectMe != Time.frameCount)
		{
			mSelectMe = -1;
			mSelectionEnd = ((!string.IsNullOrEmpty(mValue)) ? mValue.Length : 0);
			mDrawStart = 0;
			mSelectionStart = ((!selectAllTextOnFocus) ? mSelectionEnd : 0);
			label.color = activeTextColor;
			AndroidKeyboardManager.SetCursorPosition(mSelectionStart, mSelectionEnd);
			RuntimePlatform platform = Application.platform;
			if ((int)platform != 8 && (int)platform != 11 && (int)platform != 21)
			{
				Vector2 compositionCursorPos = Vector2.op_Implicit((!((Object)(object)UICamera.current != (Object)null) || !((Object)(object)UICamera.current.cachedCamera != (Object)null)) ? label.worldCorners[0] : UICamera.current.cachedCamera.WorldToScreenPoint(label.worldCorners[0]));
				compositionCursorPos.y = (float)Screen.height - compositionCursorPos.y;
				Input.imeCompositionMode = (IMECompositionMode)1;
				Input.compositionCursorPos = compositionCursorPos;
			}
			UpdateLabel();
			if (string.IsNullOrEmpty(Input.inputString))
			{
				return;
			}
		}
		if (mKeyboard != null)
		{
			Update_AndroidKeyboard(executeOnChange: false);
		}
		else
		{
			string compositionString = Input.compositionString;
			if (!string.IsNullOrEmpty(Input.inputString))
			{
				string inputString = Input.inputString;
				for (int i = 0; i < inputString.Length; i++)
				{
					char c = inputString[i];
					if (c >= ' ' && c != '\uf700' && c != '\uf701' && c != '\uf702' && c != '\uf703')
					{
						Insert(c.ToString());
					}
				}
				Input.inputString = string.Empty;
			}
			if (mLastIME != compositionString)
			{
				mLastIME = compositionString;
				UpdateLabel();
				ExecuteOnChange();
			}
		}
		if ((Object)(object)mCaret != (Object)null && mNextBlink < RealTime.time)
		{
			mNextBlink = RealTime.time + 0.5f;
			((Behaviour)mCaret).enabled = !((Behaviour)mCaret).enabled;
		}
		if ((Object)(object)mCam == (Object)null)
		{
			mCam = UICamera.FindCameraForLayer(((Component)this).gameObject.layer);
		}
		if (!((Object)(object)mCam != (Object)null))
		{
			return;
		}
		bool flag = false;
		if (label.multiLine)
		{
			bool flag2 = Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305);
			flag = ((onReturnKey != OnReturnKey.Submit) ? (!flag2) : flag2);
		}
		if (UICamera.GetKeyDown(mCam.submitKey0) || ((int)mCam.submitKey0 == 13 && UICamera.GetKeyDown((KeyCode)271)))
		{
			if (flag)
			{
				Insert("\n");
			}
			else
			{
				if ((Object)(object)UICamera.controller.current != (Object)null)
				{
					UICamera.controller.clickNotification = UICamera.ClickNotification.None;
				}
				UICamera.currentKey = mCam.submitKey0;
				Submit();
			}
		}
		if (UICamera.GetKeyDown(mCam.submitKey1) || ((int)mCam.submitKey1 == 13 && UICamera.GetKeyDown((KeyCode)271)))
		{
			if (flag)
			{
				Insert("\n");
			}
			else
			{
				if ((Object)(object)UICamera.controller.current != (Object)null)
				{
					UICamera.controller.clickNotification = UICamera.ClickNotification.None;
				}
				UICamera.currentKey = mCam.submitKey1;
				Submit();
			}
		}
		if (!mCam.useKeyboard && UICamera.GetKeyUp((KeyCode)9))
		{
			OnKey((KeyCode)9);
		}
	}

	private void OnKey(KeyCode key)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		int frameCount = Time.frameCount;
		if (mIgnoreKey == frameCount)
		{
			return;
		}
		if ((Object)(object)mCam != (Object)null && (key == mCam.cancelKey0 || key == mCam.cancelKey1))
		{
			mIgnoreKey = frameCount;
			isSelected = false;
		}
		else if ((int)key == 9)
		{
			mIgnoreKey = frameCount;
			isSelected = false;
			UIKeyNavigation component = ((Component)this).GetComponent<UIKeyNavigation>();
			if ((Object)(object)component != (Object)null)
			{
				component.OnKey((KeyCode)9);
			}
		}
	}

	protected void DoBackspace()
	{
		if (string.IsNullOrEmpty(mValue))
		{
			return;
		}
		if (mSelectionStart == mSelectionEnd)
		{
			if (mSelectionStart < 1)
			{
				return;
			}
			mSelectionEnd--;
		}
		Insert(string.Empty);
	}

	protected virtual void Insert(string text)
	{
		string leftText = GetLeftText();
		string rightText = GetRightText();
		int length = rightText.Length;
		StringBuilder stringBuilder = new StringBuilder(leftText.Length + rightText.Length + text.Length);
		stringBuilder.Append(leftText);
		int i = 0;
		for (int length2 = text.Length; i < length2; i++)
		{
			char c = text[i];
			if (c == '\b')
			{
				DoBackspace();
				continue;
			}
			if (characterLimit > 0 && stringBuilder.Length + length >= characterLimit)
			{
				break;
			}
			if (onValidate != null)
			{
				c = onValidate(stringBuilder.ToString(), stringBuilder.Length, c);
			}
			else if (validation != 0)
			{
				c = Validate(stringBuilder.ToString(), stringBuilder.Length, c);
			}
			if (c != 0)
			{
				stringBuilder.Append(c);
			}
		}
		mSelectionStart = stringBuilder.Length;
		mSelectionEnd = mSelectionStart;
		int j = 0;
		for (int length3 = rightText.Length; j < length3; j++)
		{
			char c2 = rightText[j];
			if (onValidate != null)
			{
				c2 = onValidate(stringBuilder.ToString(), stringBuilder.Length, c2);
			}
			else if (validation != 0)
			{
				c2 = Validate(stringBuilder.ToString(), stringBuilder.Length, c2);
			}
			if (c2 != 0)
			{
				stringBuilder.Append(c2);
			}
		}
		mValue = stringBuilder.ToString();
		UpdateLabel();
		ExecuteOnChange();
	}

	protected string GetLeftText()
	{
		int num = Mathf.Min(mSelectionStart, mSelectionEnd);
		return (!string.IsNullOrEmpty(mValue) && num >= 0) ? mValue.Substring(0, num) : string.Empty;
	}

	protected string GetRightText()
	{
		int num = Mathf.Max(mSelectionStart, mSelectionEnd);
		return (!string.IsNullOrEmpty(mValue) && num < mValue.Length) ? mValue.Substring(num) : string.Empty;
	}

	protected string GetSelection()
	{
		if (string.IsNullOrEmpty(mValue) || mSelectionStart == mSelectionEnd)
		{
			return string.Empty;
		}
		int num = Mathf.Min(mSelectionStart, mSelectionEnd);
		int num2 = Mathf.Max(mSelectionStart, mSelectionEnd);
		return mValue.Substring(num, num2 - num);
	}

	protected int GetCharUnderMouse()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] worldCorners = label.worldCorners;
		Ray currentRay = UICamera.currentRay;
		Plane val = default(Plane);
		((Plane)(ref val))._002Ector(worldCorners[0], worldCorners[1], worldCorners[2]);
		float num = default(float);
		return ((Plane)(ref val)).Raycast(currentRay, ref num) ? (mDrawStart + label.GetCharacterIndexAtPosition(((Ray)(ref currentRay)).GetPoint(num), precise: false)) : 0;
	}

	protected virtual void OnPress(bool isPressed)
	{
		if (!isPressed || !isSelected || !((Object)(object)label != (Object)null) || (UICamera.currentScheme != 0 && UICamera.currentScheme != UICamera.ControlScheme.Touch))
		{
			return;
		}
		if (!string.IsNullOrEmpty(Input.compositionString))
		{
			Insert(Input.compositionString);
		}
		Input.compositionString = string.Empty;
		if (inputShouldBeHidden)
		{
			if (mKeyboard == null)
			{
				OnSelectEvent_Android();
			}
		}
		else if (mKeyboard != null)
		{
			mKeyboard.text = string.Empty;
			AndroidKeyboardManager.SetText(string.Empty);
		}
		else
		{
			OnSelectEvent_Android();
		}
		AndroidKeyboardManager.ClearComposition();
		mSelectionStart = GetCharUnderMouse();
		selectionEnd = GetCharUnderMouse();
		AndroidKeyboardManager.SetCursorPosition(mSelectionStart, mSelectionEnd);
		if (!Input.GetKey((KeyCode)304) && !Input.GetKey((KeyCode)303))
		{
			selectionStart = mSelectionEnd;
		}
	}

	protected virtual void OnDrag(Vector2 delta)
	{
		if ((Object)(object)label != (Object)null && (UICamera.currentScheme == UICamera.ControlScheme.Mouse || UICamera.currentScheme == UICamera.ControlScheme.Touch))
		{
			selectionEnd = GetCharUnderMouse();
			AndroidKeyboardManager.SetCursorPosition(mSelectionStart, mSelectionEnd);
		}
	}

	private void OnDisable()
	{
		Cleanup();
	}

	protected virtual void Cleanup()
	{
		if (Object.op_Implicit((Object)(object)mHighlight))
		{
			((Behaviour)mHighlight).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)mCaret))
		{
			((Behaviour)mCaret).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)mBlankTex))
		{
			NGUITools.Destroy((Object)(object)mBlankTex);
			mBlankTex = null;
		}
	}

	public void Submit()
	{
		if (NGUITools.GetActive((Behaviour)(object)this))
		{
			mValue = value;
			if ((Object)(object)current == (Object)null)
			{
				current = this;
				EventDelegate.Execute(onSubmit);
				current = null;
			}
			SaveToPlayerPrefs(mValue);
		}
	}

	public void UpdateLabel()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Expected O, but got Unknown
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)label != (Object)null))
		{
			return;
		}
		if (mDoInit)
		{
			Init();
		}
		bool flag = isSelected;
		Compsitionstring = Input.compositionString;
		if (mKeyboard != null && mKeyboard.active && AdditionalOptions.keepKeyboardOn)
		{
			flag = true;
		}
		string text = value;
		bool flag2 = string.IsNullOrEmpty(text) && string.IsNullOrEmpty(Input.compositionString);
		label.color = ((!flag2 || flag) ? activeTextColor : mDefaultColor);
		string text2;
		if (flag2)
		{
			text2 = ((!flag) ? mDefaultText : string.Empty);
			label.alignment = mAlignment;
		}
		else
		{
			if (inputType == InputType.Password)
			{
				text2 = string.Empty;
				string text3 = "*";
				if ((Object)(object)label.bitmapFont != (Object)null && label.bitmapFont.bmFont != null && label.bitmapFont.bmFont.GetGlyph(42) == null)
				{
					text3 = "x";
				}
				int i = 0;
				for (int length = text.Length; i < length; i++)
				{
					text2 += text3;
				}
			}
			else
			{
				text2 = text;
			}
			int num = (flag ? Mathf.Min(text2.Length, cursorPosition) : 0);
			string text4 = text2.Substring(0, num);
			if (flag)
			{
				text4 += Input.compositionString;
			}
			text2 = text4 + text2.Substring(num, text2.Length - num);
			if (flag && label.overflowMethod == UILabel.Overflow.ClampContent && label.maxLineCount == 1)
			{
				int num2 = label.CalculateOffsetToFit(text2);
				if (num2 == 0)
				{
					mDrawStart = 0;
					label.alignment = mAlignment;
				}
				else if (num < mDrawStart)
				{
					mDrawStart = num;
					label.alignment = NGUIText.Alignment.Left;
				}
				else if (num2 < mDrawStart)
				{
					mDrawStart = num2;
					label.alignment = NGUIText.Alignment.Left;
				}
				else
				{
					num2 = label.CalculateOffsetToFit(text2.Substring(0, num));
					if (num2 > mDrawStart)
					{
						mDrawStart = num2;
						label.alignment = NGUIText.Alignment.Right;
					}
				}
				if (mDrawStart != 0)
				{
					text2 = text2.Substring(mDrawStart, text2.Length - mDrawStart);
				}
			}
			else
			{
				mDrawStart = 0;
				label.alignment = mAlignment;
			}
		}
		label.text = text2;
		if (flag && (mKeyboard == null || inputShouldBeHidden))
		{
			if (Input.compositionString == null)
			{
				Input.compositionString = string.Empty;
			}
			int num3 = mSelectionStart + Input.compositionString.Length - mDrawStart;
			int num4 = mSelectionEnd + Input.compositionString.Length - mDrawStart;
			if ((Object)(object)mBlankTex == (Object)null)
			{
				mBlankTex = new Texture2D(2, 2, (TextureFormat)5, false);
				for (int j = 0; j < 2; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						mBlankTex.SetPixel(k, j, Color.white);
					}
				}
				mBlankTex.Apply();
			}
			if (num3 != num4)
			{
				if ((Object)(object)mHighlight == (Object)null)
				{
					mHighlight = label.cachedGameObject.AddWidget<UITexture>();
					((Object)mHighlight).name = "Input Highlight";
					mHighlight.mainTexture = (Texture)(object)mBlankTex;
					mHighlight.fillGeometry = false;
					mHighlight.pivot = label.pivot;
					mHighlight.SetAnchor(label.cachedTransform);
				}
				else
				{
					mHighlight.pivot = label.pivot;
					mHighlight.mainTexture = (Texture)(object)mBlankTex;
					mHighlight.MarkAsChanged();
					((Behaviour)mHighlight).enabled = true;
				}
			}
			if ((Object)(object)mCaret == (Object)null)
			{
				mCaret = label.cachedGameObject.AddWidget<UITexture>();
				((Object)mCaret).name = "Input Caret";
				mCaret.mainTexture = (Texture)(object)mBlankTex;
				mCaret.fillGeometry = false;
				mCaret.pivot = label.pivot;
				mCaret.SetAnchor(label.cachedTransform);
			}
			else
			{
				mCaret.pivot = label.pivot;
				mCaret.mainTexture = (Texture)(object)mBlankTex;
				mCaret.MarkAsChanged();
				((Behaviour)mCaret).enabled = true;
			}
			if (num3 != num4)
			{
				label.PrintOverlay(num3, num4, mCaret.geometry, mHighlight.geometry, caretColor, selectionColor);
				((Behaviour)mHighlight).enabled = mHighlight.geometry.hasVertices;
			}
			else
			{
				label.PrintOverlay(num3, num4, mCaret.geometry, null, caretColor, selectionColor);
				if ((Object)(object)mHighlight != (Object)null)
				{
					((Behaviour)mHighlight).enabled = false;
				}
			}
			mNextBlink = RealTime.time + 0.5f;
			mLastAlpha = label.finalAlpha;
		}
		else
		{
			Cleanup();
		}
	}

	protected char Validate(string text, int pos, char ch)
	{
		if (validation == Validation.None || !((Behaviour)this).enabled)
		{
			return ch;
		}
		if (validation == Validation.Integer)
		{
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
			if (ch == '-' && pos == 0 && !text.Contains("-"))
			{
				return ch;
			}
		}
		else if (validation == Validation.Float)
		{
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
			if (ch == '-' && pos == 0 && !text.Contains("-"))
			{
				return ch;
			}
			if (ch == '.' && !text.Contains("."))
			{
				return ch;
			}
		}
		else if (validation == Validation.Alphanumeric)
		{
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		else if (validation == Validation.Username)
		{
			if (ch >= 'A' && ch <= 'Z')
			{
				return (char)(ch - 65 + 97);
			}
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		else
		{
			if (validation == Validation.Filename)
			{
				return ch switch
				{
					':' => '\0', 
					'/' => '\0', 
					'\\' => '\0', 
					'<' => '\0', 
					'>' => '\0', 
					'|' => '\0', 
					'^' => '\0', 
					'*' => '\0', 
					';' => '\0', 
					'"' => '\0', 
					'`' => '\0', 
					'\t' => '\0', 
					'\n' => '\0', 
					_ => ch, 
				};
			}
			if (validation == Validation.Name)
			{
				char c = ((text.Length <= 0) ? ' ' : text[Mathf.Clamp(pos, 0, text.Length - 1)]);
				char c2 = ((text.Length <= 0) ? '\n' : text[Mathf.Clamp(pos + 1, 0, text.Length - 1)]);
				if (ch >= 'a' && ch <= 'z')
				{
					if (c == ' ')
					{
						return (char)(ch - 97 + 65);
					}
					return ch;
				}
				if (ch >= 'A' && ch <= 'Z')
				{
					if (c != ' ' && c != '\'')
					{
						return (char)(ch - 65 + 97);
					}
					return ch;
				}
				switch (ch)
				{
				case '\'':
					if (c != ' ' && c != '\'' && c2 != '\'' && !text.Contains("'"))
					{
						return ch;
					}
					break;
				case ' ':
					if (c != ' ' && c != '\'' && c2 != ' ' && c2 != '\'')
					{
						return ch;
					}
					break;
				}
			}
		}
		return '\0';
	}

	protected void ExecuteOnChange()
	{
		if ((Object)(object)current == (Object)null && EventDelegate.IsValid(onChange))
		{
			current = this;
			EventDelegate.Execute(onChange);
			current = null;
		}
	}

	public void RemoveFocus()
	{
		isSelected = false;
	}

	public void SaveValue()
	{
		SaveToPlayerPrefs(mValue);
	}

	public void LoadValue()
	{
		if (!string.IsNullOrEmpty(savedAs))
		{
			string text = mValue.Replace("\\n", "\n");
			mValue = string.Empty;
			value = ((!PlayerPrefs.HasKey(savedAs)) ? text : PlayerPrefs.GetString(savedAs));
		}
	}

	public void DeleteSelection()
	{
		string leftText = GetLeftText();
		string rightText = GetRightText();
		int length = rightText.Length;
		StringBuilder stringBuilder = new StringBuilder(leftText.Length + rightText.Length);
		stringBuilder.Append(leftText);
		mSelectionStart = stringBuilder.Length;
		mSelectionEnd = mSelectionStart;
		int i = 0;
		for (int length2 = rightText.Length; i < length2; i++)
		{
			char c = rightText[i];
			if (onValidate != null)
			{
				c = onValidate(stringBuilder.ToString(), stringBuilder.Length, c);
			}
			else if (validation != 0)
			{
				c = Validate(stringBuilder.ToString(), stringBuilder.Length, c);
			}
			if (c != 0)
			{
				stringBuilder.Append(c);
			}
		}
		mValue = stringBuilder.ToString();
	}

	private void OnSelectEvent_Android()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (!keepKeyboardOn || !AndroidKeyboardManager.IsOpen())
		{
			AdditionalOptions.selectAllTextOnFocus = selectAllTextOnFocus;
			mSelectMe = -1;
			mSelectionEnd = ((!string.IsNullOrEmpty(mValue)) ? mValue.Length : 0);
			mDrawStart = 0;
			mSelectionStart = ((!selectAllTextOnFocus) ? mSelectionEnd : 0);
			label.color = activeTextColor;
			TouchScreenKeyboardType kt;
			string val;
			if (inputShouldBeHidden)
			{
				TouchScreenKeyboard.hideInput = true;
				kt = (TouchScreenKeyboardType)keyboardType;
				val = mValue;
			}
			else if (inputType == InputType.Password)
			{
				TouchScreenKeyboard.hideInput = false;
				kt = (TouchScreenKeyboardType)0;
				val = mValue;
				mSelectionStart = mSelectionEnd;
			}
			else
			{
				TouchScreenKeyboard.hideInput = false;
				kt = (TouchScreenKeyboardType)keyboardType;
				val = mValue;
				mSelectionStart = mSelectionEnd;
			}
			mWaitForKeyboard = true;
			Input.inputString = string.Empty;
			((MonoBehaviour)this).StartCoroutine(OpenSoftKeyboard(val, kt));
		}
	}

	private IEnumerator OpenSoftKeyboard(string val, TouchScreenKeyboardType kt)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		while (AndroidKeyboardManager.IsOpen())
		{
			yield return null;
		}
		if (mKeyboard == null || !mKeyboard.active)
		{
			AndroidKeyboardManager.SetCharacterLimit(characterLimit);
			mKeyboard = TouchScreenKeyboard.Open(val, kt, !inputShouldBeHidden && inputType == InputType.AutoCorrect, label.multiLine && !hideInput, inputType == InputType.Password, alert: false, defaultText);
			KeyboardMessageReceiver.instance.actionUpdate = Update_AndroidKeyboard;
			KeyboardMessageReceiver.instance.actionCursorChanged = OnCursorChanged;
			AdditionalOptions.keepKeyboardOn = keepKeyboardOn;
		}
		while (!AndroidKeyboardManager.IsOpen())
		{
			yield return null;
		}
		bool needTouchhandler = false;
		if (!inputShouldBeHidden && AdditionalOptions.softInputMode == InputAdjustType.SOFT_INPUT_ADJUST_PAN)
		{
			needTouchhandler = true;
		}
		if (needTouchhandler)
		{
			UICamera.GetInputTouchCount = GetTouchCount;
			UICamera.GetInputTouch = GetTouch;
		}
		else
		{
			UICamera.GetInputTouchCount = null;
			UICamera.GetInputTouch = null;
		}
		KeyboardMessageReceiver.instance.onKeyboardClosed = OnKeyboardClosed;
	}

	private void OnKeyboardClosed()
	{
		for (int num = 10; num >= 0; num--)
		{
			UICamera.RemoveTouch(num);
		}
		UICamera.GetInputTouchCount = null;
		UICamera.GetInputTouch = null;
		KeyboardMessageReceiver.instance.onKeyboardClosed = null;
		ExecuteOnChange();
	}

	private void Update_AndroidKeyboard(bool executeOnChange)
	{
		if (mKeyboard == null)
		{
			return;
		}
		string text = mKeyboard.text;
		if (inputShouldBeHidden)
		{
			mCached = mKeyboard.text;
			value = mKeyboard.text;
			mSelectionStart = TouchScreenKeyboard.CursorPositionStart;
			mSelectionEnd = TouchScreenKeyboard.CursorPositionEnd;
		}
		else if (mCached != text)
		{
			mCached = text;
			value = text;
		}
		if (mKeyboard.done || !mKeyboard.active)
		{
			if (!mKeyboard.wasCanceled)
			{
				Submit();
			}
			mKeyboard = null;
			isSelected = false;
			mCached = string.Empty;
			return;
		}
		string compositionString = Input.compositionString;
		if (mLastIME != compositionString)
		{
			mLastIME = compositionString;
			UpdateLabel();
			if (!executeOnChange)
			{
				ExecuteOnChange();
			}
		}
		if (executeOnChange)
		{
			ExecuteOnChange();
		}
		if (mKeyboard.active && AdditionalOptions.keepKeyboardOn && TouchScreenKeyboard.instance.OnReturnKey)
		{
			if (!mKeyboard.done && mKeyboard.active)
			{
				Submit();
				isSelected = true;
			}
			TouchScreenKeyboard.instance.OnReturnKey = false;
		}
	}

	public int GetTouchCount()
	{
		return AndroidTouch.instance.touchCount;
	}

	public UICamera.Touch GetTouch(int index)
	{
		AndroidTouch.Touch touch = AndroidTouch.instance.GetTouch(index);
		return ConvertTouch(touch);
	}

	private static UICamera.Touch ConvertTouch(AndroidTouch.Touch src)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		UICamera.Touch touch = new UICamera.Touch();
		touch.fingerId = src.fingerId;
		touch.phase = src.phase;
		touch.position = src.position;
		touch.tapCount = src.tapCount;
		return touch;
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			AndroidKeyboardManager.CloseInCode();
			OnKeyboardClosed();
			Input.inputString = string.Empty;
			Input.compositionString = string.Empty;
			mKeyboard = null;
			((MonoBehaviour)this).StopAllCoroutines();
		}
	}

	private void OnCursorChanged(int start, int end)
	{
		mSelectionStart = start;
		mSelectionEnd = end;
		UpdateLabel();
	}

	public void ClearText()
	{
		value = string.Empty;
		mValue = value;
		mLoadSavedValue = false;
		mSelectionStart = 0;
		mSelectionEnd = 0;
		Input.inputString = string.Empty;
		Input.compositionString = string.Empty;
		AndroidKeyboardManager.ClearText();
		UpdateLabel();
		label.text = string.Empty;
	}
}
