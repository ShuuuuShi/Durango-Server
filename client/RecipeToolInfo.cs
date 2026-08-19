using Durango.Logic.Item;
using L10N;
using Yaml;

public class RecipeToolInfo : SlotInfo
{
	private static readonly OrTagFilter EmptyTag = new OrTagFilter();

	private OrTagFilter _toolTags = EmptyTag;

	private int _tagLevel;

	public override string Id => "tool";

	public override int Count => 1;

	public override int TotalCount => Count;

	public override int RequiredLevel => _tagLevel;

	public override OrTagFilter RequiredTags => _toolTags;

	public override OrTagFilter RequiredMaterials => EmptyTag;

	public override SlotSourceInfo[] SlotSourceInfo => null;

	public bool ToolRequired => _toolTags.Length > 0;

	public RecipeToolInfo(SlotContainer parent)
		: base(parent)
	{
	}

	public ItemData GetSelectedItem()
	{
		return (SelectedItems.Count <= 0) ? null : SelectedItems[0];
	}

	public void Refresh(int index, OrTagFilter allowedTags = null)
	{
		SelectedItems.Clear();
		_toolTags = ((!(allowedTags != null)) ? EmptyTag : allowedTags);
		SetSlotInfo(index, T._("도구"));
		_tagLevel = ((allowedTags != null && allowedTags.Count > 0) ? allowedTags.RequiredLevel() : 0);
	}

	public override bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)
	{
		return !itemData.IsDestroyed() && itemData.HasTag(_toolTags, ignoreSubReason);
	}
}
