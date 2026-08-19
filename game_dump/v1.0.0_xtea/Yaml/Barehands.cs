using System.Collections.Generic;

namespace Yaml;

public class Barehands
{
	public string attack_type;

	public float attack_cooltime;

	public string accuracy_type;

	public string weapon_framework;

	public Dictionary<string, string> action_policies;

	public float range;

	public float critical;

	public Dictionary<string, float> atk_ratio;

	public float accuracy_ratio;
}
