using System;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ArtifactInteriorMoodItem : MonoBehaviour
{
	public enum ColorType
	{
		Normal,
		Full,
		Complexity
	}

	public class Info : IComparable<Info>
	{
		[NotNull]
		private ArtifactInteriorMood _mood;

		public int Index { get; private set; }

		public string Name => _mood.Name;

		public string Description => _mood.Description;

		public string SummaryDescription => _mood.SummaryDescription;

		public int Current { get; private set; }

		public int Max => _mood.TotalLevel;

		public Info(int index, int currentLevel, [NotNull] ArtifactInteriorMood mood)
		{
			Index = index;
			Current = currentLevel;
			_mood = mood;
		}

		public int CompareTo(Info other)
		{
			if (other == null)
			{
				return 1;
			}
			float ratio = GetRatio();
			float ratio2 = other.GetRatio();
			if (ratio != ratio2)
			{
				if (ratio > ratio2)
				{
					return -1;
				}
				return 1;
			}
			return Index.CompareTo(other.Index);
		}

		private float GetRatio()
		{
			return (float)Current / (float)Max;
		}
	}

	[SerializeField]
	private GameObject _bgDotLine;

	[SerializeField]
	private UISpriteLabel _textMood;

	[SerializeField]
	private UIProgressBar _progress;

	[SerializeField]
	private UILabel _progressCountLabel;

	[SerializeField]
	[EnumList(typeof(ColorType), false, 0, -1)]
	private Color[] _colorProgress;

	private string _interiorMoodName;

	private string _tooltipTitle;

	private string _description;

	public bool IsFullGauge { get; private set; }

	private void Awake()
	{
		UIEventListener.Get(base.gameObject).onClick = delegate
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(_tooltipTitle, _description, 400);
			widgetTooltipControl.Show(5f);
		};
	}

	public bool Set(Info info)
	{
		_interiorMoodName = info.Name;
		_tooltipTitle = T._("[FFD85BE6]{0} 분위기[-]", info.Name);
		_description = "[C0B59BE6]" + info.Description + "[-]";
		IsFullGauge = info.Current >= info.Max;
		_progress.value = Mathf.Min((float)info.Current / (float)info.Max, 1f);
		_progressCountLabel.text = $"<em>{info.Current}</em> / {info.Max}";
		_progressCountLabel.UpdateAnchors();
		SetProgressColor(IsFullGauge ? ColorType.Full : ColorType.Normal);
		SetTextLabel(IsFullGauge);
		ShowDotLine(show: true);
		return IsFullGauge;
	}

	public void ShowDotLine(bool show)
	{
		_bgDotLine.SetActive(show);
	}

	public void SetComplexity()
	{
		SetTextLabel(completed: false);
		SetProgressColor(ColorType.Complexity);
	}

	private void SetTextLabel(bool completed)
	{
		_textMood.text = ((!completed) ? _interiorMoodName : (_interiorMoodName + " [32B446FF][icon=icon_autoguidegroup_complete:1.2][-]"));
	}

	private void SetProgressColor(ColorType type)
	{
		_progress.foregroundWidget.color = _colorProgress[(int)type];
	}
}
