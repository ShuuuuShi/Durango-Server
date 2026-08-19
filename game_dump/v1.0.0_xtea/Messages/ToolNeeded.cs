using MsgPack;

namespace Messages;

public struct ToolNeeded
{
	public const uint TypeCode = 3647u;

	public string[] RecipeIds;

	public string TagNames;

	public static void Pack(Packer packer, ToolNeeded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3647u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.RecipeIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.RecipeIds.Length);
			for (int i = 0; i < val.RecipeIds.Length; i++)
			{
				if (val.RecipeIds[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.RecipeIds[i]);
				}
			}
		}
		packer.PackString(val.TagNames);
	}

	public static ToolNeeded Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		ToolNeeded result = default(ToolNeeded);
		result.RecipeIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string[] recipeIds = result.RecipeIds;
			int num2 = i;
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			recipeIds[num2] = ((MessagePackObject)(ref lastReadData2)).AsString();
		}
		unpacker.Read();
		result.TagNames = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ToolNeeded RecipeIds={RecipeIds} TagNames={TagNames}>";
	}
}
