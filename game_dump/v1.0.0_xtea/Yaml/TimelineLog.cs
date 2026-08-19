using Messages;
using Shared.System;

namespace Yaml;

public class TimelineLog
{
	public TimelineEvent Type;

	public double At;

	public ArtifactDigest? TargetArtifact;

	public ulong? TargetEntityId;

	public ulong? AgentEntityId;

	public Gettext[] Params;

	public Messages.TimelineLog ToMessage()
	{
		Messages.TimelineLog result = default(Messages.TimelineLog);
		result.Type = Type;
		result.At = At;
		result.TargetArtifact = TargetArtifact;
		result.TargetEntityId = TargetEntityId;
		result.AgentEntityId = AgentEntityId;
		string[] array = new string[Params.Length];
		for (int i = 0; i < Params.Length; i++)
		{
			array[i] = Params[i].ToString();
		}
		result.Params = array;
		return result;
	}
}
