using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct TranslatedRadioTalk
{
	public const uint TypeCode = 32978u;

	public string Text;

	public string SrcLang;

	public Dictionary<string, string> TranslatedText;

	public static void Pack(Packer packer, TranslatedRadioTalk val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(32978u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.Text == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Text);
		}
		if (val.SrcLang == null)
		{
			packer.PackNull();
		}
		else if (val.SrcLang == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SrcLang);
		}
		if (val.TranslatedText == null)
		{
			packer.PackNull();
			return;
		}
		if (val.TranslatedText == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.TranslatedText.Count);
		foreach (KeyValuePair<string, string> item in val.TranslatedText)
		{
			if (item.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(item.Key);
			}
			if (item.Value == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(item.Value);
			}
		}
	}

	public static TranslatedRadioTalk Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TranslatedRadioTalk result = default(TranslatedRadioTalk);
		result.Text = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SrcLang = null;
		}
		else
		{
			string srcLang = unpacker.LastReadData.AsString();
			result.SrcLang = srcLang;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TranslatedText = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			Dictionary<string, string> dictionary = new Dictionary<string, string>(num);
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				string key = unpacker.LastReadData.AsString();
				unpacker.Read();
				string value = unpacker.LastReadData.AsString();
				dictionary.Add(key, value);
			}
			result.TranslatedText = dictionary;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TranslatedRadioTalk Text={Text} SrcLang={SrcLang} TranslatedText={TranslatedText}>";
	}
}
