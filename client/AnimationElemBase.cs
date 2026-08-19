using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class AnimationElemBase : IClipCollectable, IClipEnumerator, IEnumerable<AnimationSequenceClip>, IEnumerable
{
	[SerializeField]
	public string key = string.Empty;

	public abstract void CollectClips(List<AnimationClip> clips);

	public abstract void AutoFill(List<string> animFbxFiles);

	public abstract void CreateNew(string frameworkName);

	public virtual bool TryMoveNext(int index, out AnimationSequenceClip res)
	{
		res = default(AnimationSequenceClip);
		return false;
	}

	public virtual IEnumerator<AnimationSequenceClip> GetEnumerator()
	{
		return new AnimationSequenceClip.Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
