using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Model;

public interface IMotionPlayable
{
	[CanBeNull]
	AnimationState GetCurAnimState();

	float Play(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f);

	float CrossFade(string motionName, float fadeTime = -1f, bool loop = true, float beginTime = 0f, float playbackRate = 1f);

	WrapMode GetWrapMode(string motionName);

	GameObject GetGameObject();

	void SetActivateRootMotion(bool active);
}
