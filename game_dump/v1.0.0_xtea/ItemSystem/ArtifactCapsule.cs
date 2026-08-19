using System.Collections.Generic;
using Messages;

namespace ItemSystem;

public class ArtifactCapsule
{
	public ulong EntityId;

	public string BlueprintId;

	public int ArtifactLevel;

	public List<TagData> Tags;

	public ArtifactDisplay Display;

	public List<KeyValuePair<string, string>> LookNames;

	public ArtifactCapsule(Messages.ArtifactCapsule capsule)
	{
		EntityId = capsule.EntityId;
		BlueprintId = capsule.BlueprintId;
		ArtifactLevel = capsule.ArtifactLevel;
		Tags = new List<TagData>();
		int i = 0;
		for (int size = KUtility.GetSize(capsule.Tags); i < size; i++)
		{
			Tags.Add(TagData.Create(capsule.Tags[i].Id, capsule.Tags[i].Level));
		}
		Display = capsule.Display;
		if (capsule.LookNames == null || capsule.LookNames.Count <= 0)
		{
			return;
		}
		LookNames = new List<KeyValuePair<string, string>>();
		foreach (KeyValuePair<string, string> lookName in capsule.LookNames)
		{
			LookNames.Add(lookName);
		}
	}

	public ArtifactCapsule(PackedArtifact pack)
	{
		EntityId = pack.EntityId;
		BlueprintId = pack.BlueprintId;
		ArtifactLevel = pack.ArtifactLevel;
		Tags = new List<TagData>();
		int i = 0;
		for (int size = KUtility.GetSize(pack.Tags); i < size; i++)
		{
			Tags.Add(TagData.Create(pack.Tags[i].Id, pack.Tags[i].Level));
		}
		Display = pack.Display;
		if (pack.LookNames == null || pack.LookNames.Count <= 0)
		{
			return;
		}
		LookNames = new List<KeyValuePair<string, string>>();
		foreach (KeyValuePair<string, string> lookName in pack.LookNames)
		{
			LookNames.Add(lookName);
		}
	}
}
