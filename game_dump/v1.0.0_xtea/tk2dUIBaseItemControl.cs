using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUIBaseItemControl")]
public abstract class tk2dUIBaseItemControl : MonoBehaviour
{
	public tk2dUIItem uiItem;

	public GameObject SendMessageTarget
	{
		get
		{
			if ((Object)(object)uiItem != (Object)null)
			{
				return uiItem.sendMessageTarget;
			}
			return null;
		}
		set
		{
			if ((Object)(object)uiItem != (Object)null)
			{
				uiItem.sendMessageTarget = value;
			}
		}
	}

	public static void ChangeGameObjectActiveState(GameObject go, bool isActive)
	{
		go.SetActive(isActive);
	}

	public static void ChangeGameObjectActiveStateWithNullCheck(GameObject go, bool isActive)
	{
		if ((Object)(object)go != (Object)null)
		{
			ChangeGameObjectActiveState(go, isActive);
		}
	}

	protected void DoSendMessage(string methodName, object parameter)
	{
		if ((Object)(object)SendMessageTarget != (Object)null && methodName.Length > 0)
		{
			SendMessageTarget.SendMessage(methodName, parameter, (SendMessageOptions)0);
		}
	}
}
