public class InputCommandInternalMessageBase
{
	public InputCommand Command;

	public bool IsDirection()
	{
		switch (Command)
		{
		case InputCommand.Up:
		case InputCommand.Down:
		case InputCommand.Left:
		case InputCommand.Right:
			return true;
		default:
			return false;
		}
	}
}
