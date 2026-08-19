using System.Collections.Generic;
using ItemSystem;
using L10N;

public class RecipeToolInfo : SlotInfo
{
	private static readonly TagFilter[] _empty = new TagFilter[0];

	private TagFilter[] _toolTags = _empty;

	public override string Id => "tool";

	public override IList<TagFilter> RequiredTags => _toolTags;

	public override IList<TagFilter> RequiredMaterials => _empty;

	public override int MaxCount => 1;

	public bool ToolRequired => _toolTags.Length > 0;

	public ItemData GetSelectedItem()
	{
		return (base.SelectedItems.Count <= 0) ? null : base.SelectedItems[0];
	}

	public void Clear()
	{
		Refresh(0, _empty);
	}

	public void Refresh(int index, TagFilter[] requiredTags)
	{
		base.SelectedItems.Clear();
		_toolTags = requiredTags;
		SetSlotInfo(index, T._("도구"), _toolTags, _empty);
		RefreshItemCount();
	}

	public override bool IsSuitableItem(ItemData itemData, bool ignoreLevel = false)
	{
		return itemData.HasTag(_toolTags, ignoreLevel);
	}
}
