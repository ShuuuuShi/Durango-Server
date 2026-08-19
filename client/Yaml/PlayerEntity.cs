using System.Collections.Generic;

namespace Yaml;

public class PlayerEntity
{
	public Barehands bare_hands;

	public string[] actions;

	public Dictionary<string, BodyParts> body_parts;

	public float bound_radius;

	public float battle_retreat_time;

	public static float BoundRadius;
}
