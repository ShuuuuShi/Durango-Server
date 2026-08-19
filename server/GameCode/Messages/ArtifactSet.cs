using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct ArtifactSet
{
	public Dictionary<string, string[]> TagSlots;

	public string SelectedId;

	public static void Pack(Packer packer, ArtifactSet val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.TagSlots == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.TagSlots.Count);
			foreach (KeyValuePair<string, string[]> tagSlot in val.TagSlots)
			{
				if (tagSlot.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(tagSlot.Key);
				}
				if (tagSlot.Value == null)
				{
					packer.PackArrayHeader(0);
					continue;
				}
				packer.PackArrayHeader(tagSlot.Value.Length);
				for (int i = 0; i < tagSlot.Value.Length; i++)
				{
					if (tagSlot.Value[i] == null)
					{
						packer.PackString(string.Empty);
					}
					else
					{
						packer.PackString(tagSlot.Value[i]);
					}
				}
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

	public static ArtifactSet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ArtifactSet result = default(ArtifactSet);
		result.TagSlots = new Dictionary<string, string[]>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			string[] array = new string[num2];
			for (int j = 0; j < num2; j++)
			{
				unpacker.Read();
				array[j] = unpacker.LastReadData.AsString();
			}
			result.TagSlots.Add(key, array);
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
		return $"<ArtifactSet TagSlots={TagSlots} SelectedId={SelectedId}>";
	}
}
