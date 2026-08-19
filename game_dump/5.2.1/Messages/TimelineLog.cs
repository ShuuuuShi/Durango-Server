using MsgPack;
using Shared.System;

namespace Messages;

public struct TimelineLog
{
	public const uint TypeCode = 2445u;

	public TimelineEvent Type;

	public double At;

	public ArtifactDigest? TargetArtifact;

	public string TargetEntityId;

	public string AgentEntityId;

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
		if (val.TargetEntityId == null)
		{
			packer.PackNull();
		}
		else if (val.TargetEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TargetEntityId);
		}
		if (val.AgentEntityId == null)
		{
			packer.PackNull();
		}
		else if (val.AgentEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.AgentEntityId);
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
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		TimelineLog result = default(TimelineLog);
		if (num < 0 || 201 < num)
		{
			result.Type = TimelineEvent.Invalid;
		}
		else
		{
			result.Type = (TimelineEvent)num;
		}
		unpacker.Read();
		result.At = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TargetArtifact = null;
		}
		else
		{
			ArtifactDigest value = ArtifactDigest.Unpack(unpacker);
			result.TargetArtifact = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TargetEntityId = null;
		}
		else
		{
			string targetEntityId = unpacker.LastReadData.AsString();
			result.TargetEntityId = targetEntityId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.AgentEntityId = null;
		}
		else
		{
			string agentEntityId = unpacker.LastReadData.AsString();
			result.AgentEntityId = agentEntityId;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
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
