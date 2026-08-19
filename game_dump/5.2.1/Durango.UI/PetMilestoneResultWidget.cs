using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Ability;
using UnityEngine;

namespace Durango.UI;

public class PetMilestoneResultWidget : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private UILabel _containerSizeLabel;

	[SerializeField]
	private UILabel _speedLabel;

	[SerializeField]
	private UIWidget _gaugeWidget;

	[SerializeField]
	private PetMilestoneGaugeResultWidget _gaugeResultWidgetBase;

	[SerializeField]
	private UILabel _attackLabel;

	[SerializeField]
	private UILabel _defenceLabel;

	[SerializeField]
	private UILabel _accuracyLabel;

	[SerializeField]
	private UILabel _attackValueLabel;

	[SerializeField]
	private UILabel _defenceValueLabel;

	[SerializeField]
	private UILabel _accuracyValueLabel;

	[SerializeField]
	private TweenerPlayer _titleTweener;

	[SerializeField]
	private TweenerPlayer _statsTweener;

	private RectLayoutComponent _layout;

	private ListObjectPool<PetMilestoneGaugeResultWidget> _gaugeResults;

	void IUIInitializable.Init()
	{
		_layout = GetComponent<RectLayoutComponent>();
		_gaugeResults = new ListObjectPool<PetMilestoneGaugeResultWidget>();
		_gaugeResults.BaseObject = _gaugeResultWidgetBase;
		_gaugeResults.UseBase = true;
		_gaugeResults.Clear();
		_attackLabel.text = T._("공격");
		_defenceLabel.text = T._("방어");
		_accuracyLabel.text = T._("명중");
	}

	public void Set(MilestoneResult result)
	{
		SetTitleStats(result);
		SetGaugeStats(result);
		SetBattleStats(result);
		_layout.UpdateLayout();
		PlayAnimation();
	}

	private void PlayAnimation()
	{
		float num = 0f;
		_titleTweener.Play(num);
		num += 0.3f;
		for (int i = 0; i < _gaugeResults.Count; i++)
		{
			_gaugeResults[i].PlayAnimation(num);
			num += 0.3f;
		}
		_statsTweener.Play(num);
	}

	private void SetTitleStats(MilestoneResult result)
	{
		if (TryGetChangedStat(result, Derived.InventoryCapacity, out var prev, out var current))
		{
			_containerSizeLabel.text = $"[icon=bg_equip_bag] {prev:0} <em>+{current - prev:0}</em>";
		}
		else
		{
			_containerSizeLabel.text = $"[icon=bg_equip_bag] {current:0}";
		}
		if (TryGetChangedStat(result, Derived.Speed, out prev, out current))
		{
			_speedLabel.text = $"[icon=icon_se_charge] {prev:0} <em>+{current - prev:0}</em>";
		}
		else
		{
			_speedLabel.text = $"[icon=icon_se_charge] {current:0}";
		}
	}

	private void SetGaugeStats(MilestoneResult result)
	{
		_gaugeResults.BeginLoad();
		if (TryGetChangedStat(result, Derived.LifeMax, out var prev, out var current))
		{
			_gaugeResults.GetNext().Set(T._("생명"), $"{prev:0} <em>+{current - prev:0}</em>", prev / current, new Color32(182, 57, 45, byte.MaxValue));
		}
		else
		{
			_gaugeResults.GetNext().Set(T._("생명"), current.ToString("0"), 1f, new Color32(182, 57, 45, byte.MaxValue));
		}
		if (TryGetChangedStat(result, Derived.LifeSpan, out prev, out current))
		{
			_gaugeResults.GetNext().Set(T._("수명"), TimedeltaFormatter.Format(prev) + " <em>+" + TimedeltaFormatter.Format(current - prev) + "</em>", prev / current, PresetColor.UIYellow);
		}
		else
		{
			_gaugeResults.GetNext().Set(T._("수명"), TimedeltaFormatter.Format(current), 1f, PresetColor.UIYellow);
		}
		if (TryGetChangedStat(result, Derived.HungryMax, out prev, out current))
		{
			_gaugeResults.GetNext().Set(T._("활력"), $"{prev} <em>+{current - prev}</em>", prev / current, PresetColor.UIYellow);
		}
		else
		{
			_gaugeResults.GetNext().Set(T._("활력"), current.ToString("0"), 1f, PresetColor.UIYellow);
		}
		_gaugeResults.EndLoad();
		float num = _gaugeResults.Reposition(Vector3.down, 25);
		_gaugeWidget.height = (int)num + 40;
	}

	private void SetBattleStats(MilestoneResult result)
	{
		if (TryGetChangedStat(result, Derived.Attack, out var prev, out var current))
		{
			_attackValueLabel.text = T._("{0:0} <em>+{1:0}</em>", prev, current - prev);
		}
		else
		{
			_attackValueLabel.text = T._("{0:0}", current);
		}
		if (TryGetChangedStat(result, Derived.Defense, out prev, out current))
		{
			_defenceValueLabel.text = T._("{0:0} <em>+{1:0}</em>", prev, current - prev);
		}
		else
		{
			_defenceValueLabel.text = T._("{0:0}", current);
		}
		if (TryGetChangedStat(result, Derived.Accuracy, out prev, out current))
		{
			_accuracyValueLabel.text = T._("{0:0} <em>+{1:0}</em>", prev, current - prev);
		}
		else
		{
			_accuracyValueLabel.text = T._("{0:0}", current);
		}
	}

	private static bool TryGetChangedStat(MilestoneResult result, Derived key, out float prev, out float current)
	{
		prev = ((result.OriginalStat != null) ? result.OriginalStat.Get(key, 0f) : 0f);
		current = ((result.NewStat != null) ? result.NewStat.Get(key, 0f) : 0f);
		return current - prev > 0f;
	}
}
