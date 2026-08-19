namespace Yaml;

public class PersonalResearch : Research
{
	private double? _duration;

	public double Duration
	{
		get
		{
			double? duration = _duration;
			if (!duration.HasValue)
			{
				_duration = StatusEffectTemplateYaml.GetStatusEffectTemplate(Effect.StatusEffectId, Effect.Level)?.GetDuration(Effect.Level) ?? 0f;
			}
			return _duration.Value;
		}
	}
}
