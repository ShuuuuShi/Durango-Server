namespace NPA;

public interface INPGCMListener : INPListenerType
{
	void OnGCMResult(int errorCode);
}
