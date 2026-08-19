using MsgPack;
using Shared.Estate;

namespace Messages;

public struct PersonalRegion
{
	public const uint TypeCode = 2042u;

	public Region Region;

	public string OwnerId;

	public int PioneerExp;

	public LicenseCategory[] AdmissionCategories;

	public static void Pack(Packer packer, PersonalRegion val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2042u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		Region.Pack(packer, val.Region);
		if (val.OwnerId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.OwnerId);
		}
		packer.Pack(val.PioneerExp);
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

	public static PersonalRegion Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PersonalRegion result = default(PersonalRegion);
		result.Region = Region.Unpack(unpacker);
		unpacker.Read();
		result.OwnerId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.PioneerExp = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
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
		return $"<PersonalRegion Region={Region} OwnerId={OwnerId} PioneerExp={PioneerExp} AdmissionCategories={AdmissionCategories}>";
	}
}
