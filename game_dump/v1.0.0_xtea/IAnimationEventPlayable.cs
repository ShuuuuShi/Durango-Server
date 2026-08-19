using UnityEngine;

public interface IAnimationEventPlayable
{
	AnimationClipInfo GetCurrentAnimationClipInfo();

	GameObject GetGameObject();

	Vector3 GetCurrentPosition();
}
