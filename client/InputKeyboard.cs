using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Durango.Logic;
using Durango.Logic.InputSystem;
using Durango.UI;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using UnityEngine;
using WindowsInput;

public class InputKeyboard : InputDispatcher<InputKeyboard.Message>
{
	public class Message : InputCommandInternalMessageBase
	{
		public KeySet KeySet;

		public Trigger CurrentTrigger;
	}

	private const KeyCode BluetoothReturn = (KeyCode)10;

	private readonly KeyCodeDictionary _keyMap = new KeyCodeDictionary();

	private readonly List<Pair<KeySet, Trigger>> _candidateKeys = new List<Pair<KeySet, Trigger>>();

	private readonly Dictionary<int, InputCommand> _menuCommands = new Dictionary<int, InputCommand>();

	private bool _init;

	public void Init()
	{
		if (!_init)
		{
			InitDefaultKey();
			InitMenuKey();
			_init = true;
		}
	}

	public void Process()
	{
		_candidateKeys.Clear();
		Layer currentLayer = GetCurrentLayer();
		Modifier pressedModifier = GetPressedModifier();
		foreach (KeyValuePair<KeySet, InputCommand> item in _keyMap)
		{
			KeySet key = item.Key;
			if (key.Modifiers == pressedModifier && (currentLayer & key.Layers) > Layer.None)
			{
				Trigger trigger = Trigger.None;
				if ((key.Trigger & Trigger.Down) != 0 && WinInput.GetKeyDown(key.Code))
				{
					trigger = Trigger.Down;
				}
				else if ((key.Trigger & Trigger.Up) != 0 && WinInput.GetKeyUp(key.Code))
				{
					trigger = Trigger.Up;
				}
				else if ((key.Trigger & Trigger.Press) != 0 && WinInput.GetKey(key.Code))
				{
					trigger = Trigger.Press;
				}
				if (trigger != 0)
				{
					PushPriorModifierKeySet(_candidateKeys, new Pair<KeySet, Trigger>(key, trigger));
				}
			}
		}
		foreach (Pair<KeySet, Trigger> candidateKey in _candidateKeys)
		{
			Dispatch(CreateMessage(candidateKey.Item1, candidateKey.Item2));
		}
	}

	public static string KeyToCaption(KeyCode keyCode)
	{
		switch (keyCode)
		{
		case KeyCode.RightShift:
		case KeyCode.LeftShift:
			return "Shift";
		case KeyCode.RightControl:
		case KeyCode.LeftControl:
			return "Ctrl";
		case KeyCode.RightAlt:
		case KeyCode.LeftAlt:
			return "Alt";
		case KeyCode.LeftWindows:
		case KeyCode.RightWindows:
			return "Win";
		case KeyCode.None:
			return " ";
		case KeyCode.Keypad0:
		case KeyCode.Keypad1:
		case KeyCode.Keypad2:
		case KeyCode.Keypad3:
		case KeyCode.Keypad4:
		case KeyCode.Keypad5:
		case KeyCode.Keypad6:
		case KeyCode.Keypad7:
		case KeyCode.Keypad8:
		case KeyCode.Keypad9:
			return ((int)(keyCode - 256)).ToString();
		default:
		{
			string text = NGUITools.KeyToCaption(keyCode);
			if (string.IsNullOrEmpty(text))
			{
				return keyCode.ToString();
			}
			return text;
		}
		case KeyCode.UpArrow:
			return "Up";
		case KeyCode.DownArrow:
			return "Down";
		case KeyCode.RightArrow:
			return "Right";
		case KeyCode.LeftArrow:
			return "Left";
		case (KeyCode)10:
		case KeyCode.Return:
		case KeyCode.KeypadEnter:
			return "Enter";
		case KeyCode.Backspace:
		case KeyCode.Pause:
		case KeyCode.Space:
		case KeyCode.N:
		case KeyCode.PageUp:
		case KeyCode.PageDown:
			return keyCode.ToString();
		}
	}

	private static void PushPriorModifierKeySet(IList<Pair<KeySet, Trigger>> candidates, Pair<KeySet, Trigger> newKeySet)
	{
		KeySet item = newKeySet.Item1;
		bool flag = false;
		bool flag2 = true;
		for (int num = candidates.Count - 1; num >= 0; num--)
		{
			KeySet item2 = candidates[num].Item1;
			if (item2.Code == item.Code)
			{
				flag2 = false;
				Modifier modifier = item.Modifiers | item2.Modifiers;
				if (modifier == item.Modifiers && modifier != item2.Modifiers)
				{
					flag = true;
					candidates.RemoveAt(num);
				}
			}
		}
		if (flag2 || flag)
		{
			candidates.Add(newKeySet);
		}
	}

	private Message CreateMessage(KeySet inputSet, Trigger currentTrigger)
	{
		Message cachedMessage = InputDispatcher<Message>.GetCachedMessage();
		cachedMessage.KeySet = inputSet;
		cachedMessage.Command = _keyMap[inputSet];
		cachedMessage.CurrentTrigger = currentTrigger;
		return cachedMessage;
	}

	public void DispatchCommand(InputCommand command, Trigger trigger)
	{
		List<KeySet> keySetList = GetKeySetList(command, trigger);
		if (keySetList != null && keySetList.Count != 0)
		{
			Dispatch(CreateMessage(keySetList[0], trigger));
		}
	}

	private void InitDefaultKey()
	{
		_keyMap.SafeAddStream(KeyCode.W, Layer.Controllable, InputCommand.Up);
		_keyMap.SafeAddStream(KeyCode.A, Layer.Controllable, InputCommand.Left);
		_keyMap.SafeAddStream(KeyCode.S, Layer.Controllable, InputCommand.Down);
		_keyMap.SafeAddStream(KeyCode.D, Layer.Controllable, InputCommand.Right);
		_keyMap.SafeAdd(KeyCode.W, Layer.MiniGame, InputCommand.Up);
		_keyMap.SafeAdd(KeyCode.A, Layer.MiniGame, InputCommand.Left);
		_keyMap.SafeAdd(KeyCode.S, Layer.MiniGame, InputCommand.Down);
		_keyMap.SafeAdd(KeyCode.D, Layer.MiniGame, InputCommand.Right);
		_keyMap.SafeAddStream(KeyCode.UpArrow, Layer.Controllable, InputCommand.Up);
		_keyMap.SafeAddStream(KeyCode.LeftArrow, Layer.Controllable, InputCommand.Left);
		_keyMap.SafeAddStream(KeyCode.DownArrow, Layer.Controllable, InputCommand.Down);
		_keyMap.SafeAddStream(KeyCode.RightArrow, Layer.Controllable, InputCommand.Right);
		_keyMap.SafeAdd((KeyCode)10, InputCommand.PopChatImmediately);
		_keyMap.SafeAdd(KeyCode.KeypadEnter, InputCommand.PopChatImmediately);
		_keyMap.SafeAdd(KeyCode.Return, InputCommand.PopChatImmediately);
		_keyMap.SafeAdd((KeyCode)10, Modifier.LeftShift, InputCommand.PopChat);
		_keyMap.SafeAdd(KeyCode.KeypadEnter, Modifier.LeftShift, InputCommand.PopChat);
		_keyMap.SafeAdd(KeyCode.Return, Modifier.LeftShift, InputCommand.PopChat);
		_keyMap.SafeAdd((KeyCode)10, Layer.TitleUI, InputCommand.SelectCurrentCell);
		_keyMap.SafeAdd(KeyCode.KeypadEnter, Layer.TitleUI, InputCommand.SelectCurrentCell);
		_keyMap.SafeAdd(KeyCode.Return, Layer.TitleUI, InputCommand.SelectCurrentCell);
		_keyMap.SafeAdd(KeyCode.A, Modifier.LeftControl, Layer.InputText, InputCommand.SelectAllChatInput);
		_keyMap.SafeAdd(KeyCode.Tab, Modifier.LeftControl, ~Layer.TitleUI, InputCommand.ChatTabSwitch);
		_keyMap.SafeAdd(KeyCode.E, Layer.GamePlay | Layer.InventoryUI, InputCommand.Collect);
		_keyMap.SafeAdd(KeyCode.E, Layer.InteractionUI, InputCommand.BeginFight);
		_keyMap.SafeAdd(KeyCode.K, Layer.Menu, InputCommand.Market);
		_keyMap.SafeAdd(KeyCode.B, Layer.Menu, InputCommand.WarpShop);
		_keyMap.SafeAdd(KeyCode.M, Layer.Menu, InputCommand.WorldMap);
		_keyMap.SafeAdd(KeyCode.C, Layer.Menu, InputCommand.Recipe);
		_keyMap.SafeAdd(KeyCode.N, Layer.Menu, InputCommand.Connect);
		_keyMap.SafeAdd(KeyCode.P, Layer.Menu, InputCommand.Character);
		_keyMap.SafeAdd(KeyCode.H, Layer.Menu, InputCommand.Screenshot);
		_keyMap.SafeAdd(KeyCode.F, Layer.Menu, InputCommand.Pet);
		_keyMap.SafeAdd(KeyCode.U, Layer.Menu, InputCommand.Music);
		_keyMap.SafeAdd(KeyCode.G, Layer.Menu, InputCommand.Encyclopedia);
		_keyMap.SafeAdd(KeyCode.Y, Layer.Menu, InputCommand.PlayerSelection);
		_keyMap.SafeAdd(KeyCode.O, Layer.Menu, InputCommand.Social);
		_keyMap.SafeAdd(KeyCode.Q, Layer.Menu, InputCommand.Skill);
		_keyMap.SafeAdd(KeyCode.R, Layer.Menu, InputCommand.RepeatLastMenu);
		_keyMap.SafeAdd(KeyCode.F12, InputCommand.ScreenCapture);
		_keyMap.SafeAdd(KeyCode.Space, Layer.GamePlay | Layer.InteractionUI | Layer.InventoryUI, InputCommand.PlayerJump);
		_keyMap.SafeAdd(KeyCode.I, Layer.Menu, InputCommand.Inventory);
		_keyMap.SafeAdd(KeyCode.Escape, Layer.Menu, InputCommand.Back);
		_keyMap.SafeAdd(KeyCode.Tab, Layer.Menu, InputCommand.ShowMenuList);
		_keyMap.SafeAdd(KeyCode.T, InputCommand.CommunicationMenuButtonAction);
		_keyMap.SafeAdd(KeyCode.Space, Layer.FullscreenUI, InputCommand.FullScreenUISpaceKey);
		_keyMap.SafeAdd(KeyCode.Space, Layer.GuideUI, Trigger.Stream, InputCommand.GuideUISpaceKeyDown);
		_keyMap.SafeAdd(KeyCode.Space, Layer.GuideUI, Trigger.Up, InputCommand.GuideUISpaceKeyUp);
		_keyMap.SafeAdd(KeyCode.Space, Layer.MiniGame, InputCommand.MiniGameSpace);
		_keyMap.SafeAdd(KeyCode.Q, Layer.FullscreenUI, Trigger.DownUp, InputCommand.PrevUIGroup);
		_keyMap.SafeAdd(KeyCode.E, Layer.FullscreenUI, Trigger.DownUp, InputCommand.NextUIGroup);
		_keyMap.SafeAdd(KeyCode.LeftAlt, Modifier.LeftAlt, InputCommand.HelperButtonAction);
		for (int i = 0; i < 8; i++)
		{
			_keyMap.SafeAdd((KeyCode)(49 + i), Trigger.DownUp, (InputCommand)(70 + i));
		}
		for (int j = 0; j < 5; j++)
		{
			_keyMap.SafeAdd((KeyCode)(282 + j), Modifier.LeftShift, Layer.InteractionUI, (InputCommand)(44 + j));
		}
		for (int k = 0; k < 6; k++)
		{
			_keyMap.SafeAdd((KeyCode)(282 + k), Layer.InteractionUI, Trigger.DownUp, (InputCommand)(49 + k));
		}
		_keyMap.SafeAdd(KeyCode.LeftBracket, Trigger.DownUp, InputCommand.PrevTab);
		_keyMap.SafeAdd(KeyCode.RightBracket, Trigger.DownUp, InputCommand.NextTab);
		_keyMap.SafeAdd(KeyCode.H, Layer.InventoryUI, InputCommand.InventoryMenuBarLock);
		_keyMap.SafeAdd(KeyCode.F, Layer.InventoryUI, InputCommand.InventoryMenuBarFilter);
		_keyMap.SafeAdd(KeyCode.E, Layer.BuildGridUI, InputCommand.BuildGridActionOK);
		_keyMap.SafeAdd(KeyCode.T, Layer.BuildGridUI, InputCommand.BuildGridActionRotation);
		_keyMap.SafeAdd(KeyCode.F8, Layer.AllButText, InputCommand.LogOut);
		_keyMap.SafeAdd(KeyCode.Space, Layer.ModalPopupUI, InputCommand.ConfirmModalPopup);
		_keyMap.SafeAdd(KeyCode.Escape, Layer.ModalPopupUI, InputCommand.CancelModalPopup);
		_keyMap.SafeAdd(KeyCode.A, Layer.ModalPopupUI, InputCommand.PrevOnModalPopup);
		_keyMap.SafeAdd(KeyCode.D, Layer.ModalPopupUI, InputCommand.NextOnModalPopup);
		_keyMap.SafeAdd(KeyCode.LeftArrow, Layer.ModalPopupUI, InputCommand.PrevOnModalPopup);
		_keyMap.SafeAdd(KeyCode.RightArrow, Layer.ModalPopupUI, InputCommand.NextOnModalPopup);
		_keyMap.SafeAdd(KeyCode.F10, Layer.AllButText, InputCommand.FindLabelSetter);
		_keyMap.SafeAdd(KeyCode.F11, Layer.AllButText, InputCommand.FindMouseOverLabel);
		_keyMap.SafeAdd(KeyCode.Keypad0, Layer.AllButText, InputCommand.TestInstrument);
		_keyMap.SafeAdd(KeyCode.F12, Modifier.LeftAlt, Layer.All, InputCommand.ScreenCaptureForEditor);
		_keyMap.SafeAdd(KeyCode.F12, Modifier.RightAlt, Layer.All, InputCommand.ScreenCaptureForEditor);
		_keyMap.SafeAdd(KeyCode.F12, Modifier.LeftShift, Layer.All, InputCommand.ScreenCaptureUIOnlyForEditor);
		_keyMap.SafeAdd(KeyCode.F12, Modifier.RightShift, Layer.All, InputCommand.ScreenCaptureUIOnlyForEditor);
		_keyMap.SafeAdd(KeyCode.F12, Modifier.LeftControl, Layer.All, InputCommand.ScreenCaptureNoUIForEditor);
		_keyMap.SafeAdd(KeyCode.F12, Modifier.RightControl, Layer.All, InputCommand.ScreenCaptureNoUIForEditor);
		_keyMap.SafeAdd(KeyCode.R, Modifier.LeftShift, InputCommand.CameraReset);
		_keyMap.SafeAdd(KeyCode.Escape, Layer.InputText, InputCommand.InputUIFocusOut);
	}

	private void InitMenuKey()
	{
		MemberInfo[] members = typeof(InputCommand).GetMembers();
		foreach (MemberInfo memberInfo in members)
		{
			object[] customAttributes = memberInfo.GetCustomAttributes(typeof(CommandMenuTypeAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				CommandMenuTypeAttribute commandMenuTypeAttribute = (CommandMenuTypeAttribute)customAttributes[0];
				if (commandMenuTypeAttribute != null && memberInfo.Name.TryEnum<InputCommand>(out var value))
				{
					_menuCommands.Add((int)commandMenuTypeAttribute.Menu, value);
				}
			}
		}
	}

	public void InitShortcut()
	{
		foreach (KeyValuePair<int, InputCommand> menuCommand in _menuCommands)
		{
			InputCommand value = menuCommand.Value;
			MenuType menuType = (MenuType)menuCommand.Key;
			GameSystem<InputSystem>.Instance().On(value, delegate
			{
				UIBase script = MenuHelper.GetScript(menuType);
				if (UIBase.CurrentUI != null && script != UIBase.CurrentUI && UIBase.CurrentUI.IsOpened)
				{
					UIBase.CloseUI();
				}
				MenuHelper.Toggle(menuType);
				MenuHelper.SetLastOpendUI(menuType, script);
			});
		}
	}

	public InputCommand GetMenuCommand(MenuType menu)
	{
		return _menuCommands.Get((int)menu, InputCommand.None);
	}

	public void SetShortcut(KeyCode shortcut, InputCommand command)
	{
		_keyMap[shortcut, Modifier.None, Layer.Default, Trigger.Down] = command;
	}

	public string GetKeyCaption(InputCommand command, Layer layer = Layer.None)
	{
		KeySet firstKeySet = GetFirstKeySet(command, layer);
		if (firstKeySet == KeySet.Invalid)
		{
			return string.Empty;
		}
		if (firstKeySet.Modifiers == Modifier.None)
		{
			return KeyToCaption(firstKeySet.Code);
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder stringBuilder = reusable;
		foreach (KeyCode item in firstKeySet.ToKeyCodes())
		{
			string value = KeyToCaption(item);
			stringBuilder.Append(value);
			stringBuilder.Append("+");
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		return stringBuilder.ToString();
	}

	[CanBeNull]
	public List<KeySet> GetKeySetList(InputCommand command, Layer layer = Layer.None)
	{
		List<KeySet> keySetList = _keyMap.GetKeySetList(command);
		if (keySetList == null || keySetList.Count == 0)
		{
			return null;
		}
		if (layer == Layer.None || layer == Layer.All)
		{
			return keySetList;
		}
		for (int num = keySetList.Count - 1; num >= 0; num--)
		{
			if ((keySetList[num].Layers & layer) == 0)
			{
				keySetList.RemoveAt(num);
			}
		}
		return keySetList;
	}

	[CanBeNull]
	public List<KeySet> GetKeySetList(InputCommand command, Trigger trigger)
	{
		List<KeySet> keySetList = _keyMap.GetKeySetList(command);
		if (keySetList == null || keySetList.Count == 0)
		{
			return null;
		}
		return keySetList.Where(delegate(KeySet keyset)
		{
			KeySet keySet = keyset;
			return (keySet.Trigger & trigger) > Trigger.None;
		}).ToList();
	}

	public KeySet GetFirstKeySet(InputCommand command, Layer layer = Layer.None)
	{
		return GetKeySetList(command, layer)?[0] ?? KeySet.Invalid;
	}

	public static Layer GetCurrentLayer()
	{
		if (WebBrowserControl.HasFocus)
		{
			return Layer.WebBrowsing;
		}
		if (UIInput.selection != null && UIInput.selection.gameObject.activeInHierarchy)
		{
			return Layer.InputText;
		}
		if (TooltipBase.HasModal() || (UIManager.MessageBox != null && UIManager.MessageBox.IsShow))
		{
			return Layer.ModalPopupUI;
		}
		if (DialogueGroup_PC.IsShow)
		{
			return Layer.GuideUI;
		}
		if (UIBase.CurrentUI is BuildGridGroupBase)
		{
			return Layer.BuildGridUI;
		}
		if (CPRGroup.IsShow || MiniGameDanceGroup.IsShow)
		{
			return Layer.MiniGame;
		}
		if (UIBase.HasOpenedFullscreenUI)
		{
			return Layer.FullscreenUI;
		}
		if (GameManager.IsTitleScene)
		{
			return Layer.TitleUI;
		}
		if (InteractionMenuListWidgetBase.IsShow)
		{
			return Layer.InteractionUI;
		}
		if (InventoryGroup.IsShow)
		{
			return Layer.InventoryUI;
		}
		return Layer.GamePlay;
	}

	private static Modifier GetPressedModifier()
	{
		Modifier modifier = Modifier.None;
		if (WinInput.GetKeyDown(KeyCode.LeftControl) || WinInput.GetKey(KeyCode.LeftControl))
		{
			modifier |= Modifier.LeftControl;
		}
		if (WinInput.GetKeyDown(KeyCode.LeftAlt) || WinInput.GetKey(KeyCode.LeftAlt))
		{
			modifier |= Modifier.LeftAlt;
		}
		if (WinInput.GetKeyDown(KeyCode.LeftShift) || WinInput.GetKey(KeyCode.LeftShift))
		{
			modifier |= Modifier.LeftShift;
		}
		if (WinInput.GetKeyDown(KeyCode.LeftCommand) || WinInput.GetKey(KeyCode.LeftCommand))
		{
			modifier |= Modifier.LeftCommand;
		}
		if (WinInput.GetKeyDown(KeyCode.RightControl) || WinInput.GetKey(KeyCode.RightControl))
		{
			modifier |= Modifier.RightControl;
		}
		if (WinInput.GetKeyDown(KeyCode.RightShift) || WinInput.GetKey(KeyCode.RightShift))
		{
			modifier |= Modifier.RightShift;
		}
		if (WinInput.GetKeyDown(KeyCode.RightAlt) || WinInput.GetKey(KeyCode.RightAlt))
		{
			modifier |= Modifier.RightAlt;
		}
		if (WinInput.GetKeyDown(KeyCode.RightCommand) || WinInput.GetKey(KeyCode.RightCommand))
		{
			modifier |= Modifier.RightCommand;
		}
		return modifier;
	}
}
