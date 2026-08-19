using MsgPack;
using Shared.AlarmNotify;

namespace Messages;

public struct AlarmNotify
{
	public const uint TypeCode = 3910u;

	public Shared.AlarmNotify.AlarmNotify Key;

	public string Text;

	public string Icon;

	public float? Duration;

	public string Uri;

	public static void Pack(Packer packer, AlarmNotify val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(3910u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.Pack((int)val.Key);
		packer.PackString(val.Text);
		if (val.Icon == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Icon);
		}
		if (!val.Duration.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Duration.Value);
		}
		if (val.Uri == null)
		{
			packer.PackNull();
		}
		else if (val.Uri == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Uri);
		}
	}

	public static AlarmNotify Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AlarmNotify result = default(AlarmNotify);
		if (num < 0 || 1 < num)
		{
			result.Key = Shared.AlarmNotify.AlarmNotify.Invalid;
		}
		else
		{
			result.Key = (Shared.AlarmNotify.AlarmNotify)num;
		}
		unpacker.Read();
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Icon = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Duration = null;
		}
		else
		{
			float value = unpacker.LastReadData.AsSingle();
			result.Duration = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Uri = null;
		}
		else
		{
			string uri = unpacker.LastReadData.AsString();
			result.Uri = uri;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AlarmNotify Key={Key} Text={Text} Icon={Icon} Duration={Duration} Uri={Uri}>";
	}
}
