using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ArtifactInteriorSetItem : MonoBehaviour
{
	public class Info : IComparable<Info>
	{
		[NotNull]
		private ArtifactInteriorSet _interiorSet;

		public int Index { get; private set; }

		[CanBeNull]
		public string[] CurrentTagIds { get; private set; }

		public string Name => _interiorSet.Name;

		public string Description => _interiorSet.Description;

		public string SummaryDescription => _interiorSet.SummaryDescription;

		public Dictionary<string, int> TagSlots => _interiorSet.TagSlots;

		public Info(int index, [CanBeNull] string[] currentTagIds, [NotNull] ArtifactInteriorSet interiorSet)
		{
			Index = index;
			CurrentTagIds = currentTagIds;
			_interiorSet = interiorSet;
		}

		public string GetTagName(string tagId)
		{
			return _interiorSet.TagNames.Get(tagId);
		}

		public int CompareTo(Info other)
		{
			if (other == null)
			{
				return 1;
			}
			int size = KUtility.GetSize(CurrentTagIds);
			int size2 = KUtility.GetSize(other.CurrentTagIds);
			return (size == size2) ? Index.CompareTo(other.Index) : ((size <= size2) ? 1 : (-1));
		}
	}

	[SerializeField]
	private GameObject _bgDotLine;

	[SerializeField]
	private UIWidget _title;

	[SerializeField]
	private UISpriteLabel _textSetName;

	[SerializeField]
	private ArtifactInteriorSetItemTag _itemTagBase;

	private ListObjectPool<ArtifactInteriorSetItemTag> _itemTags;

	private string _interiorSetName;

	private string _tooltipTitle;

	private string _description;

	private UIWidget _widget;

	public bool IsFullChecked { get; private set; }

	public bool Set(Info info)
	{
		Init();
		_interiorSetName = info.Name;
		_tooltipTitle = T._("[FFD85BE6]{0} 세트효과[-]", info.Name);
		_description = $"[C0B59BE6]{info.Description}[-]";
		_itemTags.BeginLoad();
		foreach (KeyValuePair<string, int> tagSlot in info.TagSlots)
		{
			string key = tagSlot.Key;
			string tagName = info.GetTagName(key);
			for (int i = 0; i < tagSlot.Value; i++)
			{
				ArtifactInteriorSetItemTag next = _itemTags.GetNext();
				next.Refresh(key, tagName);
				next.SetChecked(flag: false);
			}
		}
		_itemTags.EndLoad();
		UpdateLayout();
		if (info.CurrentTagIds != null)
		{
			string[] currentTagIds = info.CurrentTagIds;
			foreach (string tagId in currentTagIds)
			{
				ArtifactInteriorSetItemTag uncheckedItemTag = GetUncheckedItemTag(tagId);
				if (uncheckedItemTag != null)
				{
					uncheckedItemTag.SetChecked(flag: true);
				}
			}
		}
		IsFullChecked = _itemTags.Count == GetCheckedItemsCount();
		SetTextLabel(IsFullChecked);
		ShowDotLine(show: true);
		return IsFullChecked;
	}

	public void ShowDotLine(bool show)
	{
		Init();
		_bgDotLine.SetActive(show);
	}

	public void SetComplexity()
	{
		Init();
		SetTextLabel(completed: false);
		foreach (ArtifactInteriorSetItemTag itemTag in _itemTags)
		{
			itemTag.SetComplexity();
		}
	}

	private void Init()
	{
		if (_itemTags == null)
		{
			_itemTags = new ListObjectPool<ArtifactInteriorSetItemTag>();
			_itemTags.BaseObject = _itemTagBase;
			_itemTags.UseBase = true;
			UIEventListener.Get(_title.gameObject).onClick = delegate
			{
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set(_tooltipTitle, _description, 400);
				widgetTooltipControl.Show(5f);
			};
			_widget = GetComponent<UIWidget>();
		}
	}

	private void SetTextLabel(bool completed)
	{
		_textSetName.text = ((!completed) ? _interiorSetName : $"{_interiorSetName} [32B446FF][icon=icon_autoguidegroup_complete:1.2][-]");
	}

	private void UpdateLayout()
	{
		int height = _title.height;
		float num = UIUtility.WidgetsReposition(_itemTags, Vector3.down, new Vector3(0f, -height));
		_widget.height = height + (int)num;
	}

	private ArtifactInteriorSetItemTag GetUncheckedItemTag(string tagId)
	{
		return _itemTags.FirstOrDefault((ArtifactInteriorSetItemTag t) => !t.IsChecked && t.TagId == tagId);
	}

	private int GetCheckedItemsCount()
	{
		return _itemTags.Count((ArtifactInteriorSetItemTag t) => t.IsChecked);
	}
}
