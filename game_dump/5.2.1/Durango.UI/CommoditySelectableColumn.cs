using Durango.UI.Control;
using Shared.Market;
using UnityEngine;

namespace Durango.UI;

public class CommoditySelectableColumn : SortableColumnWidget<ProductSortField>
{
	[SerializeField]
	private ProductSortField _sortType;

	public override ProductSortField Value
	{
		get
		{
			return _sortType;
		}
		set
		{
			_sortType = value;
		}
	}

	protected override void GetStateColor(out Color normal, out Color selected)
	{
		normal = new Color32(183, 178, 167, byte.MaxValue);
		selected = PresetColor.UIYellow;
	}
}
