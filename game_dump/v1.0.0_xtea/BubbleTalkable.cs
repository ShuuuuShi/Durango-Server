using UnityEngine;

public class BubbleTalkable : MonoBehaviour, IBubbleTalkable
{
	public Transform _talkBubbleTransform;

	private Renderer _mainRenderer;

	public string[] _animPaths = new string[1] { "Assets/Models/Prologue/Train" };

	private void Start()
	{
		KSingleton<PrologueManager>.Instance().NPCFloatingGroup.Add(this, null);
		KSingleton<PrologueManager>.Instance().NPCFloatingGroup.SetNametag(this, string.Empty);
	}

	public GameObject GetGameObject()
	{
		return ((Component)this).gameObject;
	}

	public bool IsTalkerVisible()
	{
		if ((Object)(object)_mainRenderer == (Object)null)
		{
			_mainRenderer = (Renderer)(object)((Component)this).gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		}
		if (Object.op_Implicit((Object)(object)_mainRenderer))
		{
			return _mainRenderer.isVisible;
		}
		return true;
	}

	public Transform GetTalkBubbleTransform()
	{
		if (Object.op_Implicit((Object)(object)_talkBubbleTransform))
		{
			return _talkBubbleTransform;
		}
		return ((Component)this).transform;
	}

	public string GetDisplayName()
	{
		return ((Object)this).name;
	}

	public void BubbleTalk(string msg)
	{
		KSingleton<PrologueManager>.Instance().NPCFloatingGroup.ShowChatMsg(this, ConditionalText.Format(msg), null);
	}

	public string[] GetAnimPaths()
	{
		return _animPaths;
	}
}
