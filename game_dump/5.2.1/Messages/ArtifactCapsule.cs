using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct ArtifactCapsule
{
	public const uint TypeCode = 3550u;

	public string EntityId;

	public string BlueprintId;

	public int ArtifactLevel;

	public Tag[] Tags;

	public Performance[] Performance;

	public ArtifactDisplay Display;

	public ArtifactState State;

	public Dictionary<string, string> LookNames;

	public Point2? OccupySize;

	public static void Pack(Packer packer, ArtifactCapsule val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(10);
			packer.Pack(3550u);
		}
		else
		{
			packer.PackArrayHeader(9);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.BlueprintId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BlueprintId);
		}
		packer.Pack(val.ArtifactLevel);
		if (val.Tags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Tags.Length);
			for (int i = 0; i < val.Tags.Length; i++)
			{
				Tag.Pack(packer, val.Tags[i]);
			}
		}
		if (val.Performance == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Performance.Length);
			for (int j = 0; j < val.Performance.Length; j++)
			{
				Messages.Performance.Pack(packer, val.Performance[j]);
			}
		}
		ArtifactDisplay.Pack(packer, val.Display);
		ArtifactState.Pack(packer, val.State);
		if (val.LookNames == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.LookNames.Count);
			foreach (KeyValuePair<string, string> lookName in val.LookNames)
			{
				if (lookName.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(lookName.Key);
				}
				packer.PackString(lookName.Value);
			}
		}
		if (!val.OccupySize.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.OccupySize.Value.x);
		packer.Pack((ushort)val.OccupySize.Value.y);
	}

	public static ArtifactCapsule Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactCapsule result = default(ArtifactCapsule);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.BlueprintId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ArtifactLevel = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Tags = new Tag[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Tag reference = ref result.Tags[i];
			reference = Tag.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Performance = new Performance[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref Performance reference2 = ref result.Performance[j];
			reference2 = Messages.Performance.Unpack(unpacker);
		}
		unpacker.Read();
		result.Display = ArtifactDisplay.Unpack(unpacker);
		unpacker.Read();
		result.State = ArtifactState.Unpack(unpacker);
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.LookNames = new Dictionary<string, string>(num3);
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			string value = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.LookNames.Add(key, value);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.OccupySize = null;
		}
		else
		{
			unpacker.ReadUInt16(out var result2);
			Point2 value2 = default(Point2);
			value2.x = result2;
			unpacker.ReadUInt16(out result2);
			value2.y = result2;
			result.OccupySize = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactCapsule EntityId={EntityId} BlueprintId={BlueprintId} ArtifactLevel={ArtifactLevel} Tags={Tags} Performance={Performance} Display={Display} State={State} LookNames={LookNames} OccupySize={OccupySize}>";
	}
}
