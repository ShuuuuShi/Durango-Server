using UnityEngine;

public struct AnimationClipInfo
{
	public string Name;

	public AnimationState State;

	public float Time => (!(State == null)) ? Mathf.Repeat(State.time, State.length) : 0f;

	public float Length => (!(State == null)) ? State.length : 0f;

	public bool IsLoop => State != null && State.wrapMode == WrapMode.Loop;

	public float PlaybackRate => (!(State == null)) ? State.speed : 1f;
}
