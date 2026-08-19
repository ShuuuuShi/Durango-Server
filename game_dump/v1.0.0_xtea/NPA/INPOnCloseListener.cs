namespace NPA;

public interface INPOnCloseListener : INPListenerType
{
	void OnClose(NPCloseResult npResult);
}
