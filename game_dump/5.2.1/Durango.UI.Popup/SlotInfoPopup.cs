using System.Collections.Generic;
using Durango.Logic.Item;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class SlotInfoPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private SlotInfoWidget _infoWidget;

	[SerializeField]
	private SlotSourceWidget _sourceWidget;

	[SerializeField]
	private RectLayout _layout;

	[SerializeField]
	private int _minWidth;

	[SerializeField]
	private int _maxWidth;

	private string _name;

	private int _level;

	private OrTagFilter _tags;

	private OrTagFilter _materials;

	private IList<SlotSourceInfo> _sourceInfos;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void Start()
	{
		base.Start();
		_infoWidget.SearchClicked += delegate
		{
			UIManager.FindScript<MarketGroup>().OpenAndSearch(_tags, _materials, _level);
			Hide();
		};
	}

	public void Set(string title, int level, OrTagFilter tags, OrTagFilter materials, IList<SlotSourceInfo> sourceInfos)
	{
		_name = title;
		_level = level;
		_tags = tags;
		_materials = materials;
		_sourceInfos = sourceInfos;
	}

	protected override void FillData()
	{
		_titleLabel.text = _name;
		int minWidth = _minWidth;
		minWidth = Mathf.Max(minWidth, _infoWidget.Set(_tags, _materials, _level, _maxWidth));
		if (KUtility.GetSize(_sourceInfos) > 0)
		{
			_sourceWidget.gameObject.SetActive(value: true);
			_sourceWidget.Parent = this;
			minWidth = Mathf.Max(minWidth, _sourceWidget.Set(_sourceInfos, _level, _maxWidth));
		}
		else
		{
			_sourceWidget.gameObject.SetActive(value: false);
		}
		base.Widget.width = minWidth;
	}

	protected override void UpdateLayout()
	{
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		base.transform.localPosition = Vector3.zero;
	}
}
