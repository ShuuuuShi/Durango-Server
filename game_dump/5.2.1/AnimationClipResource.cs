using System.Collections.Generic;
using UnityEngine;

public class AnimationClipResource : ScriptableObject
{
	[SerializeField]
	public List<AnimationClip> Clips = new List<AnimationClip>();
}
