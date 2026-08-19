using MsgPack;

namespace Messages;

public struct MannequinDisplayInfo
{
	public string Head;

	public string Body;

	public string[] HeadColor;

	public string[] BodyColor;

	public static void Pack(Packer packer, MannequinDisplayInfo val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		if (val.Head == null)
		{
			packer.PackNull();
		}
		else if (val.Head == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Head);
		}
		if (val.Body == null)
		{
			packer.PackNull();
		}
		else if (val.Body == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Body);
		}
		if (val.HeadColor == null)
		{
			packer.PackNull();
		}
		else if (val.HeadColor == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.HeadColor.Length);
			for (int i = 0; i < val.HeadColor.Length; i++)
			{
				if (val.HeadColor[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.HeadColor[i]);
				}
			}
		}
		if (val.BodyColor == null)
		{
			packer.PackNull();
			return;
		}
		if (val.BodyColor == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.BodyColor.Length);
		for (int j = 0; j < val.BodyColor.Length; j++)
		{
			if (val.BodyColor[j] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.BodyColor[j]);
			}
		}
	}

	public static MannequinDisplayInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MannequinDisplayInfo result = default(MannequinDisplayInfo);
		if (unpacker.LastReadData.IsNil)
		{
			result.Head = null;
		}
		else
		{
			string head = unpacker.LastReadData.AsString();
			result.Head = head;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Body = null;
		}
		else
		{
			string body = unpacker.LastReadData.AsString();
			result.Body = body;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.HeadColor = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				array[i] = unpacker.LastReadData.AsString();
			}
			result.HeadColor = array;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.BodyColor = null;
		}
		else
		{
			int num2 = unpacker.LastReadData.AsInt32();
			string[] array2 = new string[num2];
			for (int j = 0; j < num2; j++)
			{
				unpacker.Read();
				array2[j] = unpacker.LastReadData.AsString();
			}
			result.BodyColor = array2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<MannequinDisplayInfo Head={Head} Body={Body} HeadColor={HeadColor} BodyColor={BodyColor}>";
	}
}
