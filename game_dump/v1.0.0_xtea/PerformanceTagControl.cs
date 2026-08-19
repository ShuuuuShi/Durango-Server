using UnityEngine;
using Yaml;

public class PerformanceTagControl : ItemTagControl
{
	[SerializeField]
	private Color _colorFgDefault;

	[SerializeField]
	private Color _colorBgDefault;

	[SerializeField]
	private Color _colorFgBuff;

	[SerializeField]
	private Color _colorBgBuff;

	[SerializeField]
	private Color _colorFgDebuff;

	[SerializeField]
	private Color _colorBgDebuff;

	private string _tooltipTitle;

	private string _tooltipDescription;

	public void SetActionSetTag(string icon, string actionName, string description)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		base.Icon = icon;
		base.IconColor = _colorFgDefault;
		base.BackgroundColor = _colorBgDefault;
		SetTooltipTexts(icon, actionName, description);
	}

	public void SetStatusEffectTag(StatusEffectTemplate template, bool negative)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		base.Icon = template.icon;
		base.Name = string.Empty;
		base.IconColor = ((!negative) ? _colorFgBuff : _colorFgDebuff);
		base.BackgroundColor = ((!negative) ? _colorBgBuff : _colorBgDebuff);
		SetTooltipTexts(template.icon, template.name, template.description);
	}

	public void SetDerivedStatTag(string statName, float value, SkillModifier skillModifier)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		base.Icon = skillModifier.icon;
		base.Name = string.Format("{0}{1}", (!(value < 0f)) ? "+" : "-", value);
		base.IconColor = _colorFgDefault;
		base.BackgroundColor = _colorBgDefault;
		SetTooltipTexts(skillModifier.icon, statName, skillModifier.description);
	}

	public void ShowTooltip()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		LineTooltipControl lineTooltipControl = UIManager.Popup.Tooltip<LineTooltipControl>();
		lineTooltipControl.Set(_tooltipTitle, _tooltipDescription);
		lineTooltipControl.MaxWidth = 400;
		lineTooltipControl.Show(base.Widget, new Vector2(0f, -20f), 3600f);
	}

	private void SetTooltipTexts(string iconName, string title, string description)
	{
		if (title == null)
		{
			title = string.Empty;
		}
		_tooltipTitle = ((!string.IsNullOrEmpty(iconName)) ? $"[{iconName}] {title}" : title);
		_tooltipDescription = ((description == null) ? string.Empty : description);
	}
}
