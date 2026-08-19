using Messages;
using Shared.Clan;

public static class MemberRoleExtension
{
	public static string GetName(this MemberRole role)
	{
		if (role.IsSuperuser())
		{
			return role.Name + " <em>[icon=crown]</em>";
		}
		return role.Name;
	}

	public static bool IsSuperuser(this MemberRole role)
	{
		return role.UserType == UserType.Root;
	}

	public static bool HasPermission(this MemberRole role, Permissions permission)
	{
		return (role.GetPermissions() & permission) != 0;
	}

	public static Permissions GetPermissions(this MemberRole role)
	{
		if (role.IsSuperuser())
		{
			return (Permissions)(-1);
		}
		return role.Permissions;
	}
}
