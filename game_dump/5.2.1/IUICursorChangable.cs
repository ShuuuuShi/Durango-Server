using Durango.UI;

internal interface IUICursorChangable
{
	bool IsCursorChangable();

	bool IsCursorSpecified(ref GameCursorType cursorType);
}
