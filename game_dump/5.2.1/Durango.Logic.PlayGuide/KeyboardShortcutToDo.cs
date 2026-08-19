using Durango.Utils.Extensions;

namespace Durango.Logic.PlayGuide;

public class KeyboardShortcutToDo : ToDoBase
{
	private readonly InputCommand _inputCommand;

	public KeyboardShortcutToDo(string type)
	{
		_inputCommand = type.ToEnum(InputCommand.None);
		if (_inputCommand != 0)
		{
			GameSystem<global::InputSystem>.Instance().On(_inputCommand, OnInputCommandReceived);
		}
	}

	private void OnInputCommandReceived(InputCommandMessage message)
	{
		GameSystem<global::InputSystem>.Instance().Off(_inputCommand, OnInputCommandReceived);
		CallComplete();
	}
}
