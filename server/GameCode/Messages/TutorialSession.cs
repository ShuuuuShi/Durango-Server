using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct TutorialSession
{
	public string SessionId;

	public string[] Players;

	public Dictionary<string, int> Materials;

	public static void Pack(Packer packer, TutorialSession val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.SessionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SessionId);
		}
		if (val.Players == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Players.Length);
			for (int i = 0; i < val.Players.Length; i++)
			{
				if (val.Players[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Players[i]);
				}
			}
		}
		if (val.Materials == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Materials.Count);
		foreach (KeyValuePair<string, int> material in val.Materials)
		{
			if (material.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(material.Key);
			}
			packer.Pack(material.Value);
		}
	}

	public static TutorialSession Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TutorialSession result = default(TutorialSession);
		result.SessionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Players = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Players[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Materials = new Dictionary<string, int>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.Materials.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TutorialSession SessionId={SessionId} Players={Players} Materials={Materials}>";
	}
}
