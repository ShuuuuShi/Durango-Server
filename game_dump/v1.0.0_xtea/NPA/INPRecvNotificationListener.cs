using SimpleJSON;

namespace NPA;

public interface INPRecvNotificationListener : INPListenerType
{
	void OnRecvNotification(JSONNode recvNotification);
}
