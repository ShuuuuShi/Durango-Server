using System;
using Messages;
using Newtonsoft.Json;
using Shared.Battle;
using UnityEngine;

namespace Yaml;

public class PlayerActionAttackInfo
{
	[JsonProperty(PropertyName = "damage_type")]
	public DamageType DamageType;

	[JsonProperty(PropertyName = "attack_time")]
	public float AttackTime;

	[JsonProperty(PropertyName = "radius")]
	public float Radius;

	[JsonProperty(PropertyName = "angles")]
	public Pair<float, float> Angles;

	[JsonProperty(PropertyName = "offset")]
	public Pair<float, float> Offset;

	[JsonProperty(PropertyName = "rect_half_size")]
	public Pair<float, float> RectHalfSize;

	[JsonProperty(PropertyName = "damage_angle")]
	public float DamageAngle;

	[JsonProperty(PropertyName = "use_target_origin")]
	public bool UseTargetOrigin;

	[JsonProperty(PropertyName = "damage_bonus")]
	public float DamageBonus;

	public AttackAlerted? MakeAlerted(Vector2 pos, float yaw, double usedAt)
	{
		float f = (90f - yaw) * ((float)Math.PI / 180f);
		Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
		Vector2 vector2 = new Vector2(vector.y, 0f - vector.x);
		pos += vector * Offset.Item2 + vector2 * Offset.Item1;
		AttackAlerted value = default(AttackAlerted);
		value.Center = new WorldPosition(pos.x, pos.y);
		value.DamageType = DamageType;
		value.EventAt = usedAt;
		value.AttackTime = usedAt + (double)AttackTime;
		value.Yaw = yaw + DamageAngle;
		switch (DamageType)
		{
		case DamageType.Melee:
		case DamageType.CircularArea:
		case DamageType.Ranged:
			value.Radius = Radius;
			if (Angles.Item1 != Angles.Item2)
			{
				value.Angles = new Vector2(Angles.Item1, Angles.Item2);
			}
			break;
		case DamageType.RectangularArea:
			if (RectHalfSize.Item1 > 0f && RectHalfSize.Item2 > 0f)
			{
				value.RectSizeHalves = new Vector2(RectHalfSize.Item1, RectHalfSize.Item2);
			}
			break;
		default:
			return null;
		}
		return value;
	}
}
