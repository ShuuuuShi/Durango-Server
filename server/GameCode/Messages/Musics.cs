using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Musics
{
	public const uint TypeCode = 47852454u;

	public Dictionary<int, Music> _Musics;

	public Dictionary<string, Music> SharedMusics;

	public static void Pack(Packer packer, Musics val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(47852454u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val._Musics == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val._Musics.Count);
			foreach (KeyValuePair<int, Music> music in val._Musics)
			{
				packer.Pack(music.Key);
				Music.Pack(packer, music.Value);
			}
		}
		if (val.SharedMusics == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.SharedMusics.Count);
		foreach (KeyValuePair<string, Music> sharedMusic in val.SharedMusics)
		{
			if (sharedMusic.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(sharedMusic.Key);
			}
			Music.Pack(packer, sharedMusic.Value);
		}
	}

	public static Musics Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Musics result = default(Musics);
		result._Musics = new Dictionary<int, Music>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int key = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			Music value = Music.Unpack(unpacker);
			result._Musics.Add(key, value);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.SharedMusics = new Dictionary<string, Music>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			string key2 = unpacker.LastReadData.AsString();
			unpacker.Read();
			Music value2 = Music.Unpack(unpacker);
			result.SharedMusics.Add(key2, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Musics _Musics={_Musics} SharedMusics={SharedMusics}>";
	}
}
