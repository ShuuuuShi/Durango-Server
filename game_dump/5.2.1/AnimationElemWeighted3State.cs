using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimationElemWeighted3State : AnimationElemBase
{
	[SerializeField]
	public List<Weighted3StateMotion> elems = new List<Weighted3StateMotion>();

	public override void CollectClips(List<AnimationClip> clips)
	{
		foreach (Weighted3StateMotion elem in elems)
		{
			elem.CollectClips(clips);
		}
	}

	public override void CreateNew(string frameworkName)
	{
		elems.Clear();
		elems.Add(new Weighted3StateMotion());
	}

	public override void AutoFill(List<string> animFbxFiles)
	{
	}

	public override IEnumerator<AnimationSequenceClip> GetEnumerator()
	{
		float num = 0f;
		foreach (Weighted3StateMotion elem in elems)
		{
			num += elem.weight;
		}
		Weighted3StateMotion weighted3StateMotion = null;
		if (num > 0f)
		{
			float value = UnityEngine.Random.value;
			float num2 = 0f;
			foreach (Weighted3StateMotion elem2 in elems)
			{
				num2 += elem2.weight / num;
				if (value < num2)
				{
					weighted3StateMotion = elem2;
					break;
				}
			}
		}
		if (weighted3StateMotion == null)
		{
			return new AnimationSequenceClip.Enumerator(this);
		}
		return new AnimationSequenceClip.Enumerator(weighted3StateMotion);
	}
}
