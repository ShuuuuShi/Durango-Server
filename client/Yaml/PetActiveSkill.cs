using System.Collections.Generic;
using System.Text;
using Durango.Utils;
using Newtonsoft.Json;
using Shared.Pet;

namespace Yaml;

public class PetActiveSkill
{
	[JsonProperty(PropertyName = "type")]
	public SkillType Type;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "category_icon")]
	public string CategoryIcon;

	[JsonProperty(PropertyName = "cooltime")]
	public double Cooltime;

	[JsonProperty(PropertyName = "duration")]
	public float Duration;

	[JsonProperty(PropertyName = "status_effect")]
	public Dictionary<string, int> StatusEffect;

	private string _description;

	[JsonProperty(PropertyName = "description")]
	public Gettext OriginDescription { private get; set; }

	[JsonProperty(PropertyName = "context")]
	public SkillContext[] Contextes
	{
		set
		{
			SkillContext skillContext = SkillContext.None;
			if (value != null)
			{
				SkillContext[] array = value;
				foreach (SkillContext skillContext2 in array)
				{
					skillContext |= skillContext2;
				}
			}
			Context = skillContext;
		}
	}

	public SkillContext Context { get; private set; }

	public string Description
	{
		get
		{
			if (!Gettext.IsEmpty(OriginDescription))
			{
				using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
				{
					StringBuilder value = reusable.Value;
					value.AppendLine(OriginDescription);
					if (StatusEffect != null)
					{
						int num = 0;
						foreach (KeyValuePair<string, int> item in StatusEffect)
						{
							StatusEffectTemplate statusEffectTemplate = StatusEffectTemplateYaml.GetStatusEffectTemplate(item.Key, item.Value);
							if (statusEffectTemplate != null)
							{
								if (num > 0)
								{
									value.Append(", ");
								}
								else
								{
									value.AppendLine("[size=10] [/size]");
								}
								value.AppendFormat("[icon={0}] {1}", statusEffectTemplate.Icon, statusEffectTemplate.Name);
								num++;
							}
						}
					}
					_description = value.ToString().Trim();
				}
				OriginDescription = null;
			}
			return _description;
		}
	}
}
