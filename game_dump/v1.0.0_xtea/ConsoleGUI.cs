using System;
using Homans.Containers;
using UnityEngine;

internal class ConsoleGUI : MonoBehaviour
{
	[SerializeField]
	private GUISkin _skin;

	[SerializeField]
	private int _linesVisible = 17;

	[SerializeField]
	private bool _showHierarchy = true;

	private int _historyScrollValue;

	private int _commandIndex;

	private string _command = string.Empty;

	private bool _returnPressed;

	private bool _isOpen;

	private string _partialCommand = string.Empty;

	private bool _moveCursorToEnd;

	private string[] _displayObjects;

	private string[] _displayComponents;

	private Vector2 _hierarchyScrollValue;

	private Vector2 _componentScrollValue;

	private int _commandLastPos;

	private int _commandLastSelectPos;

	private string[] _displayMethods;

	private Vector2 _methodScrollValue;

	private bool _wasCursorVisible;

	private GUIStyle _styleTextField;

	private bool _moveInputCursor;

	private int _startWidth;

	private int _startHeight;

	private GUIStyle _consolebgStyle;

	public bool IsOpen
	{
		get
		{
			return _isOpen;
		}
		set
		{
			_isOpen = value;
			if (_isOpen)
			{
				_wasCursorVisible = Cursor.visible;
			}
			else
			{
				Cursor.visible = _wasCursorVisible;
			}
		}
	}

	private float Scale => 1280f / (float)Screen.width;

	private int HierarchyWidth => (int)(250f / Scale);

	private GUIStyle StyleTextField
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			if (_styleTextField == null)
			{
				GUIStyle val = new GUIStyle(GUI.skin.textField);
				val.alignment = (TextAnchor)3;
				_styleTextField = val;
			}
			return _styleTextField;
		}
	}

	private void Awake()
	{
		if (!Debug.isDebugBuild)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void Start()
	{
		_displayObjects = Console.Instance.GetGameobjectsAtPath("/");
		_displayComponents = Console.Instance.GetComponentsOfGameobject("/");
		_displayMethods = Console.Instance.GetMethodsOfComponent("/");
		ResetLineInformation();
	}

	private void ResetLineInformation()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		float num = Screen.height / 2;
		num -= (float)(_skin.box.padding.top + _skin.box.padding.bottom);
		num -= (float)(_skin.box.margin.top + _skin.box.margin.bottom);
		num -= _skin.textField.CalcHeight(new GUIContent(string.Empty), 10f);
		_linesVisible = (int)(num / _skin.label.CalcHeight(new GUIContent(string.Empty), 10f));
		float num2 = Screen.width - 10;
		num2 -= (float)HierarchyWidth;
		num2 -= _skin.verticalScrollbar.CalcSize(new GUIContent(string.Empty)).x;
		Console.Instance.maxLineWidth = (int)(num2 / _skin.label.CalcSize(new GUIContent("A")).x);
	}

	private void OnGUI()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Invalid comparison between Unknown and I4
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Invalid comparison between Unknown and I4
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Invalid comparison between Unknown and I4
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Invalid comparison between Unknown and I4
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Invalid comparison between Unknown and I4
		//IL_0711: Unknown result type (might be due to invalid IL or missing references)
		//IL_0717: Invalid comparison between Unknown and I4
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Unknown result type (might be due to invalid IL or missing references)
		//IL_0728: Invalid comparison between Unknown and I4
		//IL_0783: Unknown result type (might be due to invalid IL or missing references)
		//IL_0789: Invalid comparison between Unknown and I4
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Expected O, but got Unknown
		//IL_07cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d2: Expected O, but got Unknown
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Invalid comparison between Unknown and I4
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Expected O, but got Unknown
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		if (Screen.width != _startWidth || Screen.height != _startHeight)
		{
			_startWidth = Screen.width;
			_startHeight = Screen.height;
			ResetLineInformation();
		}
		GUI.skin = _skin;
		if ((int)Event.current.type == 4 && (int)Event.current.keyCode == 13)
		{
			_returnPressed = true;
		}
		else
		{
			_returnPressed = false;
		}
		bool flag = (int)Event.current.type == 4 && (int)Event.current.keyCode == 273;
		bool flag2 = (int)Event.current.type == 4 && (int)Event.current.keyCode == 274;
		bool flag3 = (int)Event.current.type == 4 && (int)Event.current.keyCode == 27;
		if (IsOpen)
		{
			if (_consolebgStyle == null)
			{
				Texture2D val = new Texture2D(1, 1);
				val.SetPixels((Color[])(object)new Color[1] { Color.black });
				val.Apply();
				GUIStyle val2 = GUIStyle.op_Implicit("box");
				val2.normal.background = val;
				_consolebgStyle = val2;
			}
			GUI.depth = -100;
			GUILayout.BeginArea(new Rect(0f, 0f, (float)Screen.width, (float)(Screen.height / 2)), _consolebgStyle);
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUILayout.BeginVertical((GUILayoutOption[])(object)new GUILayoutOption[0]);
			CircularBuffer<string> lines = Console.Instance.Lines;
			for (int i = lines.Count() - Mathf.Min(_linesVisible, lines.Count()) - _historyScrollValue; i < lines.Count() - _historyScrollValue; i++)
			{
				GUILayout.Label(lines.GetItemAt(i), (GUILayoutOption[])(object)new GUILayoutOption[0]);
			}
			Debug.ClearDeveloperConsole();
			GUILayout.EndVertical();
			if (lines.Count() > _linesVisible)
			{
				_historyScrollValue = (int)GUILayout.VerticalScrollbar((float)_historyScrollValue, (float)_linesVisible, (float)lines.Count(), 0f, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandHeight(true) });
			}
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal((GUILayoutOption[])(object)new GUILayoutOption[0]);
			GUI.SetNextControlName("CommandTextField");
			string command = _command;
			_command = GUILayout.TextField(_command, StyleTextField, (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.ExpandHeight(true) });
			if (_command != command)
			{
				_displayObjects = Console.Instance.GetGameobjectsAtPath(_command);
				_displayComponents = Console.Instance.GetComponentsOfGameobject(_command);
				_displayMethods = Console.Instance.GetMethodsOfComponent(_command);
				TextEditor val3 = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
				if (val3 != null)
				{
					_commandLastPos = val3.cursorIndex;
					_commandLastSelectPos = val3.selectIndex;
				}
			}
			if (GUILayout.Button("Submit", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(100f / Scale) }))
			{
				_returnPressed = true;
			}
			if (GUILayout.Button("Close", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(100f / Scale) }))
			{
				IsOpen = false;
			}
			if (GUILayout.Button("Prev", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(100f / Scale) }))
			{
				flag = true;
			}
			if (GUILayout.Button("Next", (GUILayoutOption[])(object)new GUILayoutOption[1] { GUILayout.Width(100f / Scale) }))
			{
				flag2 = true;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			if ((int)Event.current.type == 7 && _moveCursorToEnd)
			{
				TextEditor val4 = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
				if (val4 != null)
				{
					val4.MoveTextEnd();
					val4.cursorIndex = val4.selectIndex;
					GUIStyle style = val4.style;
					Rect position = val4.position;
					float width = ((Rect)(ref position)).width;
					Rect position2 = val4.position;
					val4.graphicalCursorPos = style.GetCursorPixelPosition(new Rect(0f, 0f, width, ((Rect)(ref position2)).height), val4.content, val4.cursorIndex);
					_commandLastPos = val4.cursorIndex;
					_commandLastSelectPos = val4.selectIndex;
				}
				_moveCursorToEnd = false;
			}
			if (GUI.GetNameOfFocusedControl() == "CommandTextField" && _returnPressed)
			{
				string[] array = _command.Split(new string[2] { " ", "\0" }, StringSplitOptions.RemoveEmptyEntries);
				_command = string.Empty;
				for (int j = 0; j < array.Length; j++)
				{
					_command += array[j];
					if (j == 0)
					{
						_command += " ";
					}
					else if (j < array.Length - 1)
					{
						_command += "\0";
					}
				}
				Console.Instance.Eval(_command);
				_command = string.Empty;
				_commandIndex = 0;
				_displayObjects = Console.Instance.GetGameobjectsAtPath(_command);
				_displayComponents = Console.Instance.GetComponentsOfGameobject(_command);
			}
			if (GUI.GetNameOfFocusedControl() == "CommandTextField" && flag)
			{
				if (_commandIndex == 0)
				{
					_partialCommand = _command;
				}
				_commandIndex++;
				int num = Console.Instance.Commands.Count();
				if (num > 0)
				{
					if (_commandIndex > num)
					{
						_commandIndex--;
					}
					_command = Console.Instance.Commands.GetItemAt(num - 1 - (_commandIndex - 1)).Replace("\0", " ");
					_moveCursorToEnd = true;
				}
			}
			if (GUI.GetNameOfFocusedControl() == "CommandTextField" && flag2)
			{
				_commandIndex--;
				int num2 = Console.Instance.Commands.Count();
				if (_commandIndex < 0)
				{
					_commandIndex = 0;
				}
				if (num2 > 0)
				{
					_command = ((_commandIndex <= 0) ? _partialCommand : Console.Instance.Commands.GetItemAt(num2 - 1 - (_commandIndex - 1)).Replace("\0", " "));
					_moveCursorToEnd = true;
				}
			}
		}
		if (!IsOpen && (int)Event.current.type == 5 && (int)Event.current.keyCode == 96)
		{
			IsOpen = true;
			Event.current.Use();
			Event.current.type = (EventType)12;
		}
		if (IsOpen)
		{
			Cursor.visible = true;
		}
		if (IsOpen && flag3)
		{
			IsOpen = false;
		}
		if (IsOpen && (((int)Event.current.type == 8 && GUI.GetNameOfFocusedControl() != "CommandTextField") || _moveInputCursor))
		{
			GUI.FocusControl("CommandTextField");
			TextEditor val5 = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
			if (val5 != null)
			{
				val5.cursorIndex = _commandLastPos;
				val5.selectIndex = _commandLastSelectPos;
				_moveInputCursor = !_moveInputCursor;
			}
		}
	}
}
