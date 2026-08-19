using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeightedMotion
{
	[SerializeField]
	public float weight = 1f;

	[SerializeField]
	public string motion;

	[SerializeField]
	public Vector2 duration = new Vector2(1f, 1f);

	[SerializeField]
	private AnimationClip _clipObj;

	public AnimationClip Clip
	{
		get
		{
			return _clipObj;
		}
		set
		{
			_clipObj = value;
			motion = ((!(value != null)) ? string.Empty : value.name);
		}
	}

	public static WeightedMotion GetMotion(List<WeightedMotion> motions)
	{
		float num = 0f;
		foreach (WeightedMotion motion in motions)
		{
			num += motion.weight;
		}
		if (num <= 0f)
		{
			return null;
		}
		WeightedMotion result = null;
		float value = UnityEngine.Random.value;
		float num2 = 0f;
		foreach (WeightedMotion motion2 in motions)
		{
			num2 += motion2.weight / num;
			if (value < num2)
			{
				result = motion2;
				break;
			}
		}
		return result;
	}
}
