using Shared.Clan;

namespace Durango.Logic.Clan;

public struct RoleInfo
{
	public int id;

	public int grade;

	public Permissions permissions;

	public UserType user_type;

	public Gettext name;
}
