using System;
using System.Collections.Generic;
using Durango.Logic.InputSystem;
using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

public class InputSystem : GameSystem<InputSystem>
{
	public enum MoveActionType
	{
		None,
		Manual
	}

	public enum Priority
	{
		Highest,
		Higher,
		Default,
		Lower,
		Lowest
	}

	public bool AllowJoystickMove = true;

	public bool AllowVirtualStickMove = true;

	private int _prevHotControl;

	private readonly InputMessageDispatcher _inputCommandDispatcher = new InputMessageDispatcher();

	private readonly InputTouch _inputTouch = new InputTouch();

	private readonly InputDraw _inputDraw = new InputDraw();

	private readonly InputGesture _inputGesture = new InputGesture();

	private readonly InputKeyboard _inputKeyboard = new InputKeyboard();

	private readonly InputJoystick _inputJoystick = new InputJoystick();

	private readonly InputVirtualStick _inputVirtualStick = new InputVirtualStick();

	private readonly InputAxis _inputAxis = new InputAxis();

	private readonly InputMouseWheel _inputMouseWheel = new InputMouseWheel();

	private readonly InputMouse _inputMouse = new InputMouse();

	private Camera _mainCamera;

	private UIManager _uiManager;

	private ConsoleGUI _consoleGUI;

	private float? _moveLockedAt;

	private bool _drawMode;

	private bool _isFocused = true;

	private MoveActionType _movedBy;

	private static bool _isMouseButtonReversed;

	public static bool IsMouseButtonReversed
	{
		get
		{
			return _isMouseButtonReversed;
		}
		set
		{
			if (_isMouseButtonReversed != value)
			{
				_isMouseButtonReversed = value;
				if (GameSystem<InputSystem>.HasInstance())
				{
					GameSystem<InputSystem>.Instance().Mouse.RegisterCommands();
				}
			}
		}
	}

	public InputTouch Touch => _inputTouch;

	public InputGesture Gesture => _inputGesture;

	public InputKeyboard Keyboard => _inputKeyboard;

	public InputMouse Mouse => _inputMouse;

	public bool MoveLock
	{
		get
		{
			float? moveLockedAt = _moveLockedAt;
			if (!moveLockedAt.HasValue)
			{
				return false;
			}
			if (_moveLockedAt.Value > 0f)
			{
				if (Time.time < _moveLockedAt.Value)
				{
					return true;
				}
				_moveLockedAt = null;
				return false;
			}
			return true;
		}
		set
		{
			SetMoveLockTimer((!value) ? null : new float?(0f));
		}
	}

	public bool DrawMode
	{
		get
		{
			return _drawMode;
		}
		set
		{
			if (_drawMode != value)
			{
				_drawMode = value;
				if (this.DrawModeChanged != null)
				{
					this.DrawModeChanged(_drawMode);
				}
			}
		}
	}

	public event Action MoveLocked;

	public event Action DrawLineSegmentAdded
	{
		add
		{
			_inputDraw.DrawLineSegmentAdded += value;
		}
		remove
		{
			_inputDraw.DrawLineSegmentAdded -= value;
		}
	}

	public event Action<Vector3> DrawLinePointAdded
	{
		add
		{
			_inputDraw.DrawLinePointAdded += value;
		}
		remove
		{
			_inputDraw.DrawLinePointAdded -= value;
		}
	}

	public event Action<bool> DrawModeChanged;

	public static string GetKeyCaption(InputCommand command, Layer layer = Layer.None)
	{
		InputSystem inputSystem = GameSystem<InputSystem>.Instance();
		if (inputSystem == null)
		{
			return string.Empty;
		}
		return inputSystem.Keyboard.GetKeyCaption(command, layer);
	}

	private void Awake()
	{
		Singleton<GameManager>.Instance().MainSceneLoaded += MainSceneLoaded;
		_inputKeyboard.Init();
	}

	private void Start()
	{
		_inputTouch.RegisterHandler(TouchReceived);
		_inputDraw.RegisterHandler(DrawReceived);
		_inputGesture.RegisterHandler(GestureReceived);
		_inputKeyboard.RegisterHandler(KeyboardReceived);
		_inputJoystick.RegisterHandler(JoystickReceived);
		_inputAxis.RegisterHandler(AxisReceived);
		_inputVirtualStick.RegisterHandler(VirtualStickReceived);
		_inputMouseWheel.RegisterHandler(MouseWheelReceived);
		_inputMouse.RegisterHandler(MouseReceived);
		_inputKeyboard.InitShortcut();
	}

	private void Update()
	{
		if (!_isFocused)
		{
			return;
		}
		_inputTouch.ProcessBegin();
		bool flag = false;
		List<InputTouch.TouchEvent> touchEvents = _inputTouch.TouchEvents;
		if ((!DrawMode || !_inputDraw.Process(touchEvents, _uiManager)) && !_inputTouch.ProcessTouch() && !_inputTouch.ProcessPickingAction(_mainCamera) && !_inputGesture.Process(touchEvents))
		{
			if (AllowVirtualStickMove)
			{
				_inputVirtualStick.Process(touchEvents, _uiManager);
			}
			flag = true;
		}
		_inputKeyboard.Process();
		_inputJoystick.Process();
		_inputAxis.Process(flag && AllowJoystickMove);
		_inputMouseWheel.Process();
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			_inputMouse.Process(_mainCamera);
		}
	}

	private void LateUpdate()
	{
		_inputTouch.ProcessEnd();
		_movedBy = MoveActionType.None;
		_prevHotControl = GUIUtility.hotControl;
	}

	private void OnApplicationFocus(bool focus)
	{
		_isFocused = focus;
	}

	public bool MovedByManual()
	{
		return _movedBy != MoveActionType.None;
	}

	public void On(InputCommand inputCommand, Action<InputCommandMessage> callback, Priority priority = Priority.Default)
	{
		_inputCommandDispatcher.RegisterHandler(inputCommand, callback, priority);
	}

	public void Off(InputCommand inputCommand, Action<InputCommandMessage> callback, Priority priority = Priority.Default)
	{
		_inputCommandDispatcher.UnregisterHandler(inputCommand, callback, priority);
	}

	public void StopPropagation()
	{
		_inputCommandDispatcher.StopPropagation();
	}

	private void TouchReceived(InputTouch.Message message)
	{
		InputCommandMessage inputCommandMessage = InputCommandConverter.Convert(message);
		_inputCommandDispatcher.Dispatch(inputCommandMessage.Command, inputCommandMessage);
	}

	private void DrawReceived(InputDraw.Message message)
	{
		InputCommandMessage inputCommandMessage = InputCommandConverter.Convert(message);
		_inputCommandDispatcher.Dispatch(inputCommandMessage.Command, inputCommandMessage);
	}

	private void GestureReceived(InputGesture.Message message)
	{
		InputCommandMessage inputCommandMessage = InputCommandConverter.Convert(message);
		_inputCommandDispatcher.Dispatch(inputCommandMessage.Command, inputCommandMessage);
	}

	private void KeyboardReceived(InputKeyboard.Message message)
	{
		if (CheckPassMoveMessage(message) && (!(_consoleGUI != null) || !_consoleGUI.IsOpen || !Cursor.visible))
		{
			if (message.IsDirection() && InputKeyboard.GetCurrentLayer() != Layer.MiniGame)
			{
				_inputAxis.KeyboardPressed(message);
				return;
			}
			InputCommandMessage inputCommandMessage = InputCommandConverter.Convert(message);
			_inputCommandDispatcher.Dispatch(inputCommandMessage.Command, inputCommandMessage);
		}
	}

	private void JoystickReceived(InputJoystick.Message message)
	{
		if (CheckPassMoveMessage(message) && !message.IsDirection())
		{
			InputCommandMessage inputCommandMessage = InputCommandConverter.Convert(message);
			_inputCommandDispatcher.Dispatch(inputCommandMessage.Command, inputCommandMessage);
		}
	}

	private void AxisReceived(InputAxis.Message message)
	{
		if (CheckPassMoveMessage(message))
		{
			_movedBy = MoveActionType.Manual;
			InputCommandMessage inputCommandMessage = InputCommandConverter.Convert(message);
			_inputCommandDispatcher.Dispatch(inputCommandMessage.Command, inputCommandMessage);
		}
	}

	private void VirtualStickReceived(InputVirtualStick.Message message)
	{
		if (CheckPassMoveMessage(message))
		{
			_movedBy = MoveActionType.Manual;
			InputCommandMessage inputCommandMessage = InputCommandConverter.Convert(message);
			_inputCommandDispatcher.Dispatch(inputCommandMessage.Command, inputCommandMessage);
		}
	}

	private void MouseWheelReceived(InputMouseWheel.Message message)
	{
		InputCommandMessage inputCommandMessage = InputCommandConverter.Convert(message);
		_inputCommandDispatcher.Dispatch(inputCommandMessage.Command, inputCommandMessage);
	}

	private void MouseReceived(InputMouse.Message message)
	{
		InputCommandMessage inputCommandMessage = InputCommandConverter.Convert(message);
		_inputCommandDispatcher.Dispatch(inputCommandMessage.Command, inputCommandMessage);
	}

	private bool HasHotControl()
	{
		return GUIUtility.hotControl != 0 || _prevHotControl != 0;
	}

	private bool CheckPassMoveMessage(InputCommandInternalMessageBase message)
	{
		if (HasHotControl())
		{
			return false;
		}
		return !message.IsDirection() || _movedBy == MoveActionType.None;
	}

	private void MainSceneLoaded()
	{
		_mainCamera = Singleton<MainCamera>.Instance().GetComponent<Camera>();
		_uiManager = Singleton<UIManager>.Instance();
		_consoleGUI = UnityEngine.Object.FindObjectOfType<ConsoleGUI>();
	}

	public void MoveLockTimer(float duration)
	{
		SetMoveLockTimer(duration);
	}

	private void SetMoveLockTimer(float? duration)
	{
		if (!duration.HasValue)
		{
			_moveLockedAt = null;
			return;
		}
		if (duration.Value > 0f)
		{
			_moveLockedAt = ((!duration.HasValue) ? null : new float?(Time.time + duration.GetValueOrDefault()));
		}
		else
		{
			_moveLockedAt = 0f;
		}
		if (this.MoveLocked != null)
		{
			this.MoveLocked();
		}
	}
}
