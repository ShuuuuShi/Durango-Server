using System;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Ability;
using UnityEngine;

namespace Durango.UI;

public class CharacterBasicStatusWidget : UIWidget
{
	[Serializable]
	private struct BasicStatus
	{
		[LocalizableString]
		public string Tooltip;

		public Basic Ability;
	}

	[SerializeField]
	private KeyValueLabel _baesStatWidget;

	[SerializeField]
	private BasicStatus[] _abilities;

	private ListObjectPool<KeyValueLabel> _stats;

	private bool _init;

	public void Init()
	{
		if (_init)
		{
			return;
		}
		_init = true;
		_stats = new ListObjectPool<KeyValueLabel>();
		_stats.BaseObject = _baesStatWidget;
		_stats.UseBase = true;
		_stats.Set(KUtility.GetSize(_abilities));
		for (int i = 0; i < _stats.Count; i++)
		{
			KeyValueLabel keyValueLabel = _stats[i];
			keyValueLabel.SetKey(_abilities[i].Ability.GetName());
			Transform transform = _stats[i].transform.Find("Question");
			if (!(transform == null))
			{
				string text = T._(_abilities[i].Tooltip);
				UIEventListener.Get(transform.gameObject).onClick = delegate(GameObject go)
				{
					ShowTooltip(go, text);
				};
			}
		}
	}

	public void Refresh()
	{
		if (_init)
		{
			Statistics? statistics = GameSystem<StatisticsSystem>.Instance().Statistics;
			for (int i = 0; i < _stats.Count; i++)
			{
				_stats[i].SetValue((statistics.HasValue ? statistics.Value.BasicAbilities.Get(_abilities[i].Ability, 0) : 0).ToString());
			}
		}
	}

	public void UpdateLayout(int maxWidth, bool isPortrait)
	{
		if (_init)
		{
			base.transform.localPosition = new Vector3(22f, -24f, 0f);
			Vector2 vector = new Vector2(34f, 28f);
			Vector2 vector2 = new Vector2(_baesStatWidget.Widget.width, _baesStatWidget.Widget.height);
			Vector2 vector3 = new Vector2(42f, 22f);
			int num = ((!isPortrait) ? 4 : 2);
			int num2 = _stats.Count / num;
			base.width = maxWidth - 44;
			base.height = (int)(vector2.y * (float)num2 + vector3.y * (float)(num2 - 1) + vector.y * 2f);
			vector2.x = ((float)base.width - vector3.x * (float)(num - 1) - vector.x * 2f) / (float)num;
			for (int i = 0; i < _stats.Count; i++)
			{
				int num3 = i % num;
				int num4 = i / num;
				KeyValueLabel keyValueLabel = _stats[i];
				float x = vector.x + (vector2.x + vector3.x) * (float)num3;
				float num5 = vector.y + (vector2.y + vector3.y) * (float)num4;
				keyValueLabel.Widget.width = (int)vector2.x;
				keyValueLabel.Widget.SetPosition(new Vector2(x, 0f - num5), 0f, 1f);
			}
			UIUtility.UpdateAnchors(base.transform);
		}
	}

	private void ShowTooltip(GameObject go, string text)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(null, text);
		widgetTooltipControl.Sign = -1;
		widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
		widgetTooltipControl.Show(go, Vector2.zero);
	}
}
