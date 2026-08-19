using Durango.Terrain;
using JetBrains.Annotations;

public abstract class NaturalObject : ImmovableBase
{
	public abstract void OnRemoved([CanBeNull] TerrainChunkBase chunk, bool fastRemove);

	public override string GetName()
	{
		string text = DataHelper.GetBiomeSpriteInfo(base.EntityType)?.Name;
		return (!string.IsNullOrEmpty(text)) ? text : base.EntityType.ToString();
	}
}
