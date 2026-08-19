using MsgPack;
using Shared.Estate;

namespace Messages;

public struct SetPersonalRegionAdmission
{
	public const uint TypeCode = 20423u;

	public LicenseCategory[] AdmissionCategories;

	public static void Pack(Packer packer, SetPersonalRegionAdmission val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(20423u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.AdmissionCategories == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.AdmissionCategories.Length);
		for (int i = 0; i < val.AdmissionCategories.Length; i++)
		{
			packer.Pack((int)val.AdmissionCategories[i]);
		}
	}

	public static SetPersonalRegionAdmission Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SetPersonalRegionAdmission result = default(SetPersonalRegionAdmission);
		result.AdmissionCategories = new LicenseCategory[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			if (num2 < 0 || 3 < num2)
			{
				result.AdmissionCategories[i] = LicenseCategory.Invalid;
			}
			else
			{
				result.AdmissionCategories[i] = (LicenseCategory)num2;
			}
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetPersonalRegionAdmission AdmissionCategories={AdmissionCategories}>";
	}
}
