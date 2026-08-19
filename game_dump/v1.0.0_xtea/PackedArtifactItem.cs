using Building_;
using ItemSystem;
using UnityEngine;

public class PackedArtifactItem : SelectableWidget
{
	[SerializeField]
	private UISprite _iconSprite;

	public void Set(ArtifactCapsule pack)
	{
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(pack.BlueprintId);
		_iconSprite.spriteName = blueprint.Icon;
	}
}
