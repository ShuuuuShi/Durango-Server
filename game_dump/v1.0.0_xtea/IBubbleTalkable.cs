using UnityEngine;

public interface IBubbleTalkable
{
	GameObject GetGameObject();

	bool IsTalkerVisible();

	Transform GetTalkBubbleTransform();

	string GetDisplayName();

	string[] GetAnimPaths();
}
