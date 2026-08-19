using System;
using System.Collections.Generic;
using System.Text;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ArtifactStatsInfo : UIWidget
{
	public Action Closed;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private UILabel _statsHeadKeyLabel;

	[SerializeField]
	private UILabel _statsHeadValueLabel;

	[SerializeField]
	private UIWidget _statsBodyWigdet;

	[SerializeField]
	private UILabel _statsBodyKeyLabel;

	[SerializeField]
	private UILabel _statsBodyValueLabel;

	[SerializeField]
	private RectLayout _layout;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIEventListener uIEventListener = UIEventListener.Get(_titleWidget.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnBack));
			UIPanel componentInParent = GetComponentInParent<UIPanel>();
			_scrollView.Panel.depth = componentInParent.depth + 1;
			_layout.UpdateLayout();
		}
	}

	public void Set([NotNull] Artifact artifact, string title, string head, Func<ArtifactStats, int> getFactor, Func<ArtifactStats, int> getComplexity)
	{
		Init();
		_titleLabel.text = title;
		if (!artifact.ArtifactState.Stats.HasValue)
		{
			return;
		}
		ArtifactStats value = artifact.ArtifactState.Stats.Value;
		_statsHeadKeyLabel.text = head;
		_statsHeadValueLabel.text = getFactor(value).ToString();
		IEnumerable<Artifact> interiors = artifact.GetInteriors();
		if (interiors != null)
		{
			using Reusable<HashSet<string>> reusable3 = ReusableHashSet<string>.Pop();
			using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
			using Reusable<StringBuilder> reusable2 = ReusableStringBuilder.Pop();
			StringBuilder value2 = reusable.Value;
			StringBuilder value3 = reusable2.Value;
			foreach (Artifact item in interiors)
			{
				if (!(item == null) && item.ArtifactState.Stats.HasValue && !reusable3.Value.Contains(item.EntityId))
				{
					ArtifactStats value4 = item.ArtifactState.Stats.Value;
					if (getFactor(value4) != 0)
					{
						reusable3.Value.Add(item.EntityId);
						value2.AppendLine(item.LocalizedName);
						value3.AppendLine(getFactor(value4).ToString("+0;-0;0"));
					}
				}
			}
			int num = getComplexity(value);
			if ((float)Mathf.Abs(num) > 0f)
			{
				value2.AppendLine(T._("복잡함"));
				value3.AppendLine($"<alert>{num:+0;-0;0}</alert>");
			}
			_statsBodyKeyLabel.text = value2.ToString().Trim();
			_statsBodyValueLabel.text = value3.ToString().Trim();
			_statsBodyWigdet.height = Mathf.Max(_statsBodyKeyLabel.height, _statsBodyValueLabel.height) + 40;
			_statsBodyWigdet.gameObject.SetActive(value: true);
		}
		else
		{
			_statsBodyWigdet.gameObject.SetActive(value: false);
		}
		_scrollView.ResetPosition();
	}

	private void OnBack(GameObject obj)
	{
		if (Closed != null)
		{
			Closed();
		}
	}
}
