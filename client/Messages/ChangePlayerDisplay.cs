using MsgPack;

namespace Messages;

public struct ChangePlayerDisplay
{
	public const uint TypeCode = 34589u;

	public string ItemId;

	public string Gender;

	public string Hair;

	public string[] BodyColor;

	public string[] HeadColor;

	public string SkinColor;

	public string HairColor;

	public string LipColor;

	public string EyeColor;

	public int Portrait;

	public int PortraitBg;

	public string PortraitBgColor;

	public string Beard;

	public int VoiceType;

	public float BodySize;

	public static void Pack(Packer packer, ChangePlayerDisplay val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(16);
			packer.Pack(34589u);
		}
		else
		{
			packer.PackArrayHeader(15);
		}
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
		if (val.Gender == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Gender);
		}
		if (val.Hair == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Hair);
		}
		if (val.BodyColor == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.BodyColor.Length);
			for (int i = 0; i < val.BodyColor.Length; i++)
			{
				if (val.BodyColor[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.BodyColor[i]);
				}
			}
		}
		if (val.HeadColor == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.HeadColor.Length);
			for (int j = 0; j < val.HeadColor.Length; j++)
			{
				if (val.HeadColor[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.HeadColor[j]);
				}
			}
		}
		if (val.SkinColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkinColor);
		}
		if (val.HairColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.HairColor);
		}
		if (val.LipColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.LipColor);
		}
		if (val.EyeColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EyeColor);
		}
		packer.Pack(val.Portrait);
		packer.Pack(val.PortraitBg);
		if (val.PortraitBgColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PortraitBgColor);
		}
		if (val.Beard == null)
		{
			packer.PackNull();
		}
		else if (val.Beard == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Beard);
		}
		packer.Pack(val.VoiceType);
		packer.Pack(val.BodySize);
	}

	public static ChangePlayerDisplay Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ChangePlayerDisplay result = default(ChangePlayerDisplay);
		result.ItemId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Gender = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Hair = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.BodyColor = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.BodyColor[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.HeadColor = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.HeadColor[j] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		result.SkinColor = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.HairColor = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.LipColor = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EyeColor = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Portrait = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.PortraitBg = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.PortraitBgColor = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Beard = null;
		}
		else
		{
			string beard = unpacker.LastReadData.AsString();
			result.Beard = beard;
		}
		unpacker.Read();
		result.VoiceType = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.BodySize = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<ChangePlayerDisplay ItemId={ItemId} Gender={Gender} Hair={Hair} BodyColor={BodyColor} HeadColor={HeadColor} SkinColor={SkinColor} HairColor={HairColor} LipColor={LipColor} EyeColor={EyeColor} Portrait={Portrait} PortraitBg={PortraitBg} PortraitBgColor={PortraitBgColor} Beard={Beard} VoiceType={VoiceType} BodySize={BodySize}>";
	}
}
