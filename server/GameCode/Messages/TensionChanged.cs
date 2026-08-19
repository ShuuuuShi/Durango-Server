using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct TensionChanged
{
	public const uint TypeCode = 3794u;

	public string EntityId;

	public double EventAt;

	public string TensionChangedText;

	public Dictionary<string, string> RenderEffects;

	public static void Pack(Packer packer, TensionChanged val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3794u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EventAt);
		packer.PackString(val.TensionChangedText);
		if (val.RenderEffects == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.RenderEffects.Count);
		foreach (KeyValuePair<string, string> renderEffect in val.RenderEffects)
		{
			if (renderEffect.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(renderEffect.Key);
			}
			if (renderEffect.Value == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(renderEffect.Value);
			}
		}
	}

	public static TensionChanged Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TensionChanged result = default(TensionChanged);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EventAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.TensionChangedText = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.RenderEffects = new Dictionary<string, string>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			string value = unpacker.LastReadData.AsString();
			result.RenderEffects.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TensionChanged EntityId={EntityId} EventAt={EventAt} TensionChangedText={TensionChangedText} RenderEffects={RenderEffects}>";
	}
}
