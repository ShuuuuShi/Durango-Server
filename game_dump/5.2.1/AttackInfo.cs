using System;
using Shared.Battle;
using UnityEngine;

[Serializable]
public class AttackInfo
{
	[SerializeField]
	public int frame;

	[SerializeField]
	public string sub_action_id = string.Empty;

	[SerializeField]
	public DamageType damage_type;

	[SerializeField]
	public float radius = 100f;

	[SerializeField]
	public float radius_min;

	[SerializeField]
	public Vector2 angles;

	[SerializeField]
	public Vector2 rect_half_size;

	[SerializeField]
	public Vector2 offset;

	[SerializeField]
	public float damage_angle;

	[SerializeField]
	public bool use_target_origin;

	public AttackInfo Clone()
	{
		return (AttackInfo)MemberwiseClone();
	}
}
