using MsgPack;

namespace Messages;

public struct TutorialBoatMaterialUpdated
{
	public const uint TypeCode = 2418u;

	public string SessionId;

	public string PlayerName;

	public Pair<string, int>[] Materials;

	public static void Pack(Packer packer, TutorialBoatMaterialUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2418u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.SessionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SessionId);
		}
		if (val.PlayerName == null)
		{
			packer.PackNull();
		}
		else if (val.PlayerName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PlayerName);
		}
		if (val.Materials == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Materials.Length);
		for (int i = 0; i < val.Materials.Length; i++)
		{
			packer.PackArrayHeader(2);
			packer.PackString(val.Materials[i].Item1);
			packer.Pack(val.Materials[i].Item2);
		}
	}

	public static TutorialBoatMaterialUpdated Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TutorialBoatMaterialUpdated result = default(TutorialBoatMaterialUpdated);
		result.SessionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PlayerName = null;
		}
		else
		{
			string playerName = unpacker.LastReadData.AsString();
			result.PlayerName = playerName;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Materials = new Pair<string, int>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			string item = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			unpacker.Read();
			int item2 = unpacker.LastReadData.AsInt32();
			ref Pair<string, int> reference = ref result.Materials[i];
			reference = new Pair<string, int>(item, item2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TutorialBoatMaterialUpdated SessionId={SessionId} PlayerName={PlayerName} Materials={Materials}>";
	}
}
