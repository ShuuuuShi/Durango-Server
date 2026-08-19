using System;
using System.Collections;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUITextInput")]
[ExecuteInEditMode]
public class tk2dUITextInput : MonoBehaviour
{
	public tk2dUIItem selectionBtn;

	public tk2dTextMesh inputLabel;

	public tk2dTextMesh emptyDisplayLabel;

	public GameObject unSelectedStateGO;

	public GameObject selectedStateGO;

	public GameObject cursor;

	public float fieldLength = 1f;

	public int maxCharacterLength = 30;

	public string emptyDisplayText;

	public bool isPasswordField;

	public string passwordChar = "*";

	[HideInInspector]
	[SerializeField]
	private tk2dUILayout layoutItem;

	private bool isSelected;

	private bool wasStartedCalled;

	private bool wasOnAnyPressEventAttached;

	private TouchScreenKeyboard keyboard;

	private bool listenForKeyboardText;

	private bool isDisplayTextShown;

	public Action<tk2dUITextInput> OnTextChange;

	public string SendMessageOnTextChangeMethodName = string.Empty;

	private string text = string.Empty;

	public tk2dUILayout LayoutItem
	{
		get
		{
			return layoutItem;
		}
		set
		{
			if ((Object)(object)layoutItem != (Object)(object)value)
			{
				if ((Object)(object)layoutItem != (Object)null)
				{
					layoutItem.OnReshape -= LayoutReshaped;
				}
				layoutItem = value;
				if ((Object)(object)layoutItem != (Object)null)
				{
					layoutItem.OnReshape += LayoutReshaped;
				}
			}
		}
	}

	public GameObject SendMessageTarget
	{
		get
		{
			if ((Object)(object)selectionBtn != (Object)null)
			{
				return selectionBtn.sendMessageTarget;
			}
			return null;
		}
		set
		{
			if ((Object)(object)selectionBtn != (Object)null && (Object)(object)selectionBtn.sendMessageTarget != (Object)(object)value)
			{
				selectionBtn.sendMessageTarget = value;
			}
		}
	}

	public bool IsFocus => isSelected;

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			if (text != value)
			{
				text = value;
				if (text.Length > maxCharacterLength)
				{
					text = text.Substring(0, maxCharacterLength);
				}
				FormatTextForDisplay(text);
				if (isSelected)
				{
					SetCursorPosition();
				}
			}
		}
	}

	private void Awake()
	{
		SetState();
		ShowDisplayText();
	}

	private void Start()
	{
		wasStartedCalled = true;
		if ((Object)(object)tk2dUIManager.Instance__NoCreate != (Object)null)
		{
			tk2dUIManager.Instance.OnAnyPress += AnyPress;
		}
		wasOnAnyPressEventAttached = true;
	}

	private void OnEnable()
	{
		if (wasStartedCalled && !wasOnAnyPressEventAttached && (Object)(object)tk2dUIManager.Instance__NoCreate != (Object)null)
		{
			tk2dUIManager.Instance.OnAnyPress += AnyPress;
		}
		if ((Object)(object)layoutItem != (Object)null)
		{
			layoutItem.OnReshape += LayoutReshaped;
		}
		selectionBtn.OnClick += InputSelected;
	}

	private void OnDisable()
	{
		if ((Object)(object)tk2dUIManager.Instance__NoCreate != (Object)null)
		{
			tk2dUIManager.Instance.OnAnyPress -= AnyPress;
			if (listenForKeyboardText)
			{
				tk2dUIManager.Instance.OnInputUpdate -= ListenForKeyboardTextUpdate;
			}
		}
		wasOnAnyPressEventAttached = false;
		selectionBtn.OnClick -= InputSelected;
		listenForKeyboardText = false;
		if ((Object)(object)layoutItem != (Object)null)
		{
			layoutItem.OnReshape -= LayoutReshaped;
		}
	}

	public void SetFocus()
	{
		SetFocus(focus: true);
	}

	public void SetFocus(bool focus)
	{
		if (!IsFocus && focus)
		{
			InputSelected();
		}
		else if (IsFocus && !focus)
		{
			InputDeselected();
		}
	}

	private void FormatTextForDisplay(string modifiedText)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (isPasswordField)
		{
			int length = modifiedText.Length;
			char paddingChar = ((passwordChar.Length <= 0) ? '*' : passwordChar[0]);
			modifiedText = string.Empty;
			modifiedText = modifiedText.PadRight(length, paddingChar);
		}
		inputLabel.text = modifiedText;
		inputLabel.Commit();
		while (true)
		{
			Bounds bounds = ((Component)inputLabel).GetComponent<Renderer>().bounds;
			if (!(((Bounds)(ref bounds)).extents.x * 2f > fieldLength))
			{
				break;
			}
			modifiedText = modifiedText.Substring(1, modifiedText.Length - 1);
			inputLabel.text = modifiedText;
			inputLabel.Commit();
		}
		if (modifiedText.Length == 0 && !listenForKeyboardText)
		{
			ShowDisplayText();
		}
		else
		{
			HideDisplayText();
		}
	}

	private void ListenForKeyboardTextUpdate()
	{
		bool flag = false;
		string text = this.text;
		string inputString = Input.inputString;
		foreach (char c in inputString)
		{
			if (c == "\b"[0])
			{
				if (this.text.Length != 0)
				{
					text = this.text.Substring(0, this.text.Length - 1);
					flag = true;
				}
			}
			else if (c != "\n"[0] && c != "\r"[0] && c != '\t' && c != '\u001b')
			{
				text += c;
				flag = true;
			}
		}
		if (flag)
		{
			Text = text;
			if (OnTextChange != null)
			{
				OnTextChange(this);
			}
			if ((Object)(object)SendMessageTarget != (Object)null && SendMessageOnTextChangeMethodName.Length > 0)
			{
				SendMessageTarget.SendMessage(SendMessageOnTextChangeMethodName, (object)this, (SendMessageOptions)0);
			}
		}
	}

	private void InputSelected()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (text.Length == 0)
		{
			HideDisplayText();
		}
		isSelected = true;
		if (!listenForKeyboardText)
		{
			tk2dUIManager.Instance.OnInputUpdate += ListenForKeyboardTextUpdate;
		}
		listenForKeyboardText = true;
		SetState();
		SetCursorPosition();
		if ((int)Application.platform != 7 && (int)Application.platform != 0)
		{
			TouchScreenKeyboard.hideInput = false;
			keyboard = TouchScreenKeyboard.Open(text, (TouchScreenKeyboardType)0, false, false, isPasswordField, false);
			((MonoBehaviour)this).StartCoroutine(TouchScreenKeyboardLoop());
		}
	}

	private IEnumerator TouchScreenKeyboardLoop()
	{
		while (keyboard != null && !keyboard.done && keyboard.active)
		{
			Text = keyboard.text;
			yield return null;
		}
		if (keyboard != null)
		{
			Text = keyboard.text;
		}
		if (isSelected)
		{
			InputDeselected();
		}
	}

	private void InputDeselected()
	{
		if (text.Length == 0)
		{
			ShowDisplayText();
		}
		isSelected = false;
		if (listenForKeyboardText)
		{
			tk2dUIManager.Instance.OnInputUpdate -= ListenForKeyboardTextUpdate;
		}
		listenForKeyboardText = false;
		SetState();
		if (keyboard != null && !keyboard.done)
		{
			keyboard.active = false;
		}
		keyboard = null;
	}

	private void AnyPress()
	{
		if (isSelected && (Object)(object)tk2dUIManager.Instance.PressedUIItem != (Object)(object)selectionBtn)
		{
			InputDeselected();
		}
	}

	private void SetState()
	{
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(unSelectedStateGO, !isSelected);
		tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(selectedStateGO, isSelected);
		tk2dUIBaseItemControl.ChangeGameObjectActiveState(cursor, isSelected);
	}

	private void SetCursorPosition()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Invalid comparison between Unknown and I4
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Invalid comparison between Unknown and I4
		float num = 1f;
		float num2 = 0.002f;
		if ((int)inputLabel.anchor == 3 || (int)inputLabel.anchor == 6 || (int)inputLabel.anchor == 0)
		{
			num = 2f;
		}
		else if ((int)inputLabel.anchor == 5 || (int)inputLabel.anchor == 8 || (int)inputLabel.anchor == 2)
		{
			num = -2f;
			num2 = 0.012f;
		}
		if (text.EndsWith(" "))
		{
			tk2dFontChar tk2dFontChar2 = ((!inputLabel.font.inst.useDictionary) ? inputLabel.font.inst.chars[32] : inputLabel.font.inst.charDict[32]);
			num2 += tk2dFontChar2.advance * inputLabel.scale.x / 2f;
		}
		Transform transform = cursor.transform;
		float x = ((Component)inputLabel).transform.localPosition.x;
		Bounds bounds = ((Component)inputLabel).GetComponent<Renderer>().bounds;
		transform.localPosition = new Vector3(x + (((Bounds)(ref bounds)).extents.x + num2) * num, cursor.transform.localPosition.y, cursor.transform.localPosition.z);
	}

	private void ShowDisplayText()
	{
		if (!isDisplayTextShown)
		{
			isDisplayTextShown = true;
			if ((Object)(object)emptyDisplayLabel != (Object)null)
			{
				emptyDisplayLabel.text = emptyDisplayText;
				emptyDisplayLabel.Commit();
				tk2dUIBaseItemControl.ChangeGameObjectActiveState(((Component)emptyDisplayLabel).gameObject, isActive: true);
			}
			tk2dUIBaseItemControl.ChangeGameObjectActiveState(((Component)inputLabel).gameObject, isActive: false);
		}
	}

	private void HideDisplayText()
	{
		if (isDisplayTextShown)
		{
			isDisplayTextShown = false;
			tk2dUIBaseItemControl.ChangeGameObjectActiveStateWithNullCheck(((Component)emptyDisplayLabel).gameObject, isActive: false);
			tk2dUIBaseItemControl.ChangeGameObjectActiveState(((Component)inputLabel).gameObject, isActive: true);
		}
	}

	private void LayoutReshaped(Vector3 dMin, Vector3 dMax)
	{
		fieldLength += dMax.x - dMin.x;
		string text = this.text;
		this.text = string.Empty;
		Text = text;
	}
}
