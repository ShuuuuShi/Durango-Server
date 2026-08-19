using Messages;

public static class ArtifactAccessExtension
{
	public static bool CheckClanRole(this ArtifactAccess access, int roleId)
	{
		if (access.ClanMembers != null)
		{
			return access.ClanMembers.Get(roleId, defaultValue: false);
		}
		return false;
	}
}
