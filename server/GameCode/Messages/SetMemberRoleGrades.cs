using MsgPack;

namespace Messages;

public struct SetMemberRoleGrades
{
	public const uint TypeCode = 792252u;

	public RoleOrder[] RoleOrders;

	public static void Pack(Packer packer, SetMemberRoleGrades val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(792252u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.RoleOrders == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.RoleOrders.Length);
		for (int i = 0; i < val.RoleOrders.Length; i++)
		{
			RoleOrder.Pack(packer, val.RoleOrders[i]);
		}
	}

	public static SetMemberRoleGrades Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SetMemberRoleGrades result = default(SetMemberRoleGrades);
		result.RoleOrders = new RoleOrder[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref RoleOrder reference = ref result.RoleOrders[i];
			reference = RoleOrder.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetMemberRoleGrades RoleOrders={RoleOrders}>";
	}
}
