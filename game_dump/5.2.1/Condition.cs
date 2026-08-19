using System;
using UnityEngine;

[Serializable]
public class Condition
{
	[SerializeField]
	public string flag = "leg_injury";

	[SerializeField]
	public string enemy_distance_with_bound = "<1000";
}
