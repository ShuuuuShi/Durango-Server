using UnityEngine;

namespace K1Network;

public abstract class ConnectionHook : MonoBehaviour
{
	public abstract bool HookSendingMessage(object msg);
}
