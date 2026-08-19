using MsgPack;

namespace Messages;

public struct NotificationAdded
{
	public const uint TypeCode = 3715u;

	public ulong Id;

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
		packer.Pack(val.Id);
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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		NotificationAdded result = default(NotificationAdded);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.Since = null;
		}
		else
		{
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			double value = ((MessagePackObject)(ref lastReadData3)).AsDouble();
			result.Since = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.Until = null;
		}
		else
		{
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			double value2 = ((MessagePackObject)(ref lastReadData5)).AsDouble();
			result.Until = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.Period = ((MessagePackObject)(ref lastReadData6)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.Text = ((MessagePackObject)(ref lastReadData7)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<NotificationAdded Id={Id} Since={Since} Until={Until} Period={Period} Text={Text}>";
	}
}
