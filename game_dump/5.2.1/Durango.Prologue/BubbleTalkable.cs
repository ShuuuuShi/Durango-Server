using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class BubbleTalkable : MonoBehaviour, IBubbleTalkable
{
	public Transform _talkBubbleTransform;

	private Renderer _mainRenderer;

	public string[] _animPaths = new string[1] { "Assets/Models/Prologue/Train" };

	private void Start()
	{
		Singleton<PrologueManager>.Instance().NPCFloatingGroup.Add(this, null);
		Singleton<PrologueManager>.Instance().NPCFloatingGroup.SetNametag(this, string.Empty);
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public bool IsTalkerVisible()
	{
		if (_mainRenderer == null)
		{
			_mainRenderer = base.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		}
		if ((bool)_mainRenderer)
		{
			return _mainRenderer.isVisible;
		}
		return true;
	}

	public Transform GetTalkBubbleTransform()
	{
		if ((bool)_talkBubbleTransform)
		{
			return _talkBubbleTransform;
		}
		return base.transform;
	}

	public string GetDisplayName()
	{
		return base.name;
	}

	public void BubbleTalk(string msg)
	{
		Singleton<PrologueManager>.Instance().NPCFloatingGroup.ShowChatMsg(this, ConditionalText.Format(msg), null);
	}

	public string[] GetAnimPaths()
	{
		return _animPaths;
	}
}
