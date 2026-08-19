public class InputCommandInternalMessageBase
{
	public InputCommand Command;

	public bool IsDirection()
	{
		InputCommand command = Command;
		if ((uint)(command - 7) <= 3u)
		{
			return true;
		}
		return false;
	}
}
