using System.Collections.Generic;
using Shared.StatusEffect;

namespace Yaml;

public class StatusEffectTemplate
{
	public Gettext name;

	public Gettext description;

	public string icon;

	public string icon_color;

	public float? duration;

	public EffectType type;

	public int stack_size;

	public string visual_effect;

	public string motion;

	public Dictionary<string, float> effects;
}
