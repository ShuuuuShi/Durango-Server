using Messages;

public static class ArtifactAccessExtension
{
	public static bool CheckClanRole(this ArtifactAccess access, int roleId)
	{
		return access.ClanMembers != null && access.ClanMembers.Get(roleId, defaultValue: false);
	}
}
