using MsgPack;
using Shared.System;

namespace Messages;

public struct TimelineLog
{
	public const uint TypeCode = 2445u;

	public TimelineEvent Type;

	public double At;

	public ArtifactDigest? TargetArtifact;

	public ulong? TargetEntityId;

	public ulong? AgentEntityId;

	public string[] Params;

	public static void Pack(Packer packer, TimelineLog val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(2445u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		packer.Pack((int)val.Type);
		packer.Pack(val.At);
		if (!val.TargetArtifact.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			ArtifactDigest.Pack(packer, val.TargetArtifact.Value);
		}
		if (!val.TargetEntityId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.TargetEntityId.Value);
		}
		if (!val.AgentEntityId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.AgentEntityId.Value);
		}
		if (val.Params == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Params.Length);
		for (int i = 0; i < val.Params.Length; i++)
		{
			packer.PackString(val.Params[i]);
		}
	}

	public static TimelineLog Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		TimelineLog result = default(TimelineLog);
		if (num < 0 || 118 < num)
		{
			result.Type = TimelineEvent.Invalid;
		}
		else
		{
			result.Type = (TimelineEvent)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.At = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.TargetArtifact = null;
		}
		else
		{
			ArtifactDigest value = ArtifactDigest.Unpack(unpacker);
			result.TargetArtifact = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.TargetEntityId = null;
		}
		else
		{
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			ulong value2 = ((MessagePackObject)(ref lastReadData5)).AsUInt64();
			result.TargetEntityId = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData6)).IsNil)
		{
			result.AgentEntityId = null;
		}
		else
		{
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			ulong value3 = ((MessagePackObject)(ref lastReadData7)).AsUInt64();
			result.AgentEntityId = value3;
		}
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData8)).AsInt32();
		result.Params = new string[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			result.Params[i] = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TimelineLog Type={Type} At={At} TargetArtifact={TargetArtifact} TargetEntityId={TargetEntityId} AgentEntityId={AgentEntityId} Params={Params}>";
	}
}
