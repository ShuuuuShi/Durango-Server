using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct ArtifactMood
{
	public Dictionary<string, int> TagLevels;

	public string SelectedId;

	public static void Pack(Packer packer, ArtifactMood val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.TagLevels == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.TagLevels.Count);
			foreach (KeyValuePair<string, int> tagLevel in val.TagLevels)
			{
				if (tagLevel.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(tagLevel.Key);
				}
				packer.Pack(tagLevel.Value);
			}
		}
		if (val.SelectedId == null)
		{
			packer.PackNull();
		}
		else if (val.SelectedId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SelectedId);
		}
	}

	public static ArtifactMood Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ArtifactMood result = default(ArtifactMood);
		result.TagLevels = new Dictionary<string, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.TagLevels.Add(key, value);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SelectedId = null;
		}
		else
		{
			string selectedId = unpacker.LastReadData.AsString();
			result.SelectedId = selectedId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactMood TagLevels={TagLevels} SelectedId={SelectedId}>";
	}
}
