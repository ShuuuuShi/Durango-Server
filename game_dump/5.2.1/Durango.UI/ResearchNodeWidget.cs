using System.Text;
using Durango.Logic;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ResearchNodeWidget : SelectableWidget
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private GameObject _activeEffect;

	[SerializeField]
	private GameObject _remainTimeObject;

	[SerializeField]
	private UILabel _remainTimeLabel;

	private PersonalResearch _research;

	public string Key { get; private set; }

	public int? PioneerGrade { get; private set; }

	public void Set(string key, int? pioneerGrade, [NotNull] PersonalResearch research)
	{
		Key = key;
		PioneerGrade = pioneerGrade;
		_research = research;
		_nameLabel.text = research.Name;
		StatusEffectTemplate statusEffectTemplate = StatusEffectTemplateYaml.GetStatusEffectTemplate(research.Effect.StatusEffectId, research.Effect.Level);
		using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
		{
			StringBuilder value = reusable.Value;
			value.AppendLine(T._("{0} 지속", TimedeltaFormatter.Format(research.Duration)));
			if (statusEffectTemplate != null)
			{
				StatusEffect.EffectsText(value, statusEffectTemplate.GetEffects(research.Effect.Level));
			}
			_infoLabel.text = value.ToString();
		}
		_iconSprite.spriteName = research.Icon;
		UpdateResearchState();
		GetComponent<RectLayoutComponent>().UpdateLayout();
	}

	private void UpdateResearchState()
	{
		StatusEffect statusEffect = ((_research != null) ? GameSystem<StatusEffectSystem>.Instance().GetStatusEffect(_research.Effect.StatusEffectId, _research.Effect.Level) : null);
		if (statusEffect == null)
		{
			_activeEffect.gameObject.SetActive(value: false);
			_remainTimeObject.gameObject.SetActive(value: false);
			return;
		}
		_activeEffect.gameObject.SetActive(value: true);
		_remainTimeObject.gameObject.SetActive(value: true);
		double endAt = statusEffect.Until;
		_remainTimeLabel.SetText(new SyncString(delegate(out string text, out float period)
		{
			SyncString.UpdateRemainTimeMsg(endAt, out text, out period, string.Empty);
		}));
	}
}
