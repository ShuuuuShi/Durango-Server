using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Utils;
using L10N;
using NCalc;
using Yaml;

namespace Building;

public class BlueprintSlot : ItemSlot
{
	public Expression SizeFactor;

	public Dictionary<string, ArtifactLook> Looks;

	public string DefaultLook;

	public bool HasLook
	{
		get
		{
			if (Looks != null)
			{
				return Looks.Count > 0;
			}
			return false;
		}
	}

	public BlueprintSlot(Yaml.BlueprintSlot info)
	{
		base.Id = info.slot_id;
		base.Name = info.slot_name;
		base.Count = info.count;
		SizeFactor = ExpressionParser.Parse(info.size_factor);
		base.AllowedTags = new OrTagFilter(info.required_tags);
		base.AllowedMaterials = new OrTagFilter(info.required_materials);
		base.RequiredLevel = OrTagFilter.GetLevelFromFirstTag(base.AllowedTags, base.AllowedMaterials);
		base.SlotSourceInfos = info.source_info;
		Looks = info.looks;
		DefaultLook = info.default_look_tag;
	}

	public override string ToString()
	{
		return base.Name + " " + T._("{0}개 필요", base.Count.ToString());
	}

	public int GetSlotCountModifier(Point2 size)
	{
		if (SizeFactor == null)
		{
			return 1;
		}
		Expression sizeFactor = SizeFactor;
		sizeFactor.Parameters["x"] = size.x;
		sizeFactor.Parameters["y"] = size.y;
		return sizeFactor.ToInt32(1);
	}
}
