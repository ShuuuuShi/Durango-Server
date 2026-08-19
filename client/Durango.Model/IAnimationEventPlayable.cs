using UnityEngine;

namespace Durango.Model;

public interface IAnimationEventPlayable
{
	bool AnimationEventProhibited { get; }

	AnimationClipInfo GetCurrentAnimationClipInfo();

	GameObject GetGameObject();

	Vector3 GetCurrentPosition();
}
