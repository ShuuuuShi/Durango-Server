using System.Collections.Generic;
using Building;
using Messages;

namespace Yaml;

public class ModularArtifactContent : CommodityContent
{
	public string prototype_id;

	public int level;

	public string artifact_id;

	public int size_x;

	public int size_y;

	public float durability;

	public Dictionary<string, string> overridden_parts;

	public Dictionary<string, string> overridden_textures;

	private ArtifactDisplay? _cachedDisplay;

	public ArtifactDisplay GetPreview()
	{
		ArtifactDisplay? cachedDisplay = _cachedDisplay;
		if (!cachedDisplay.HasValue)
		{
			Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(artifact_id);
			ArtifactDisplay value;
			if (blueprint == null)
			{
				value = default(ArtifactDisplay);
			}
			else
			{
				value = blueprint.GetDefaultDisplay();
				if (overridden_parts != null)
				{
					if (value.Parts == null)
					{
						value.Parts = overridden_parts;
					}
					else
					{
						foreach (KeyValuePair<string, string> overridden_part in overridden_parts)
						{
							value.Parts[overridden_part.Key] = overridden_part.Value;
						}
					}
				}
				if (overridden_textures != null)
				{
					if (value.Textures == null)
					{
						value.Textures = overridden_textures;
					}
					else
					{
						foreach (KeyValuePair<string, string> overridden_texture in overridden_textures)
						{
							value.Textures[overridden_texture.Key] = overridden_texture.Value;
						}
					}
				}
			}
			_cachedDisplay = value;
		}
		return _cachedDisplay.Value;
	}
}
