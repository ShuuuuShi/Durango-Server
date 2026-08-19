using UnityEngine;

public interface IMotionPlayable
{
	AnimationState GetCurAnimState();

	float Play(string motionName, bool loop = true, float beginTime = 0f, float playbackRate = 1f);

	float CrossFade(string motionName, float fadeTime = -1f, bool loop = true, float beginTime = 0f, float playbackRate = 1f);

	void SetDefaultMotionName(string motionName);

	string GetDefaultMotionName();

	WrapMode GetWrapMode(string motionName);

	GameObject GetGameObject();

	void SetServerSideRootMotionEnable(bool serverSideRootMotionEnabled);
}
