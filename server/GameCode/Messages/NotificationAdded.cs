using MsgPack;

namespace Messages;

public struct NotificationAdded
{
	public const uint TypeCode = 3715u;

	public string Id;

	public double? Since;

	public double? Until;

	public float Period;

	public string Text;

	public static void Pack(Packer packer, NotificationAdded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(3715u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		if (!val.Since.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Since.Value);
		}
		if (!val.Until.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Until.Value);
		}
		packer.Pack(val.Period);
		if (val.Text == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Text);
		}
	}

	public static NotificationAdded Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		NotificationAdded result = default(NotificationAdded);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Since = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.Since = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Until = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.Until = value2;
		}
		unpacker.Read();
		result.Period = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.Text = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<NotificationAdded Id={Id} Since={Since} Until={Until} Period={Period} Text={Text}>";
	}
}
