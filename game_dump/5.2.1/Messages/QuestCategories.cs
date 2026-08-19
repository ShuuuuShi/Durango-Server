using MsgPack;

namespace Messages;

public struct QuestCategories
{
	public const uint TypeCode = 237902u;

	public QuestCategory[] Categories;

	public QuestCategory? Epic;

	public static void Pack(Packer packer, QuestCategories val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(237902u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Categories == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Categories.Length);
			for (int i = 0; i < val.Categories.Length; i++)
			{
				QuestCategory.Pack(packer, val.Categories[i]);
			}
		}
		if (!val.Epic.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			QuestCategory.Pack(packer, val.Epic.Value);
		}
	}

	public static QuestCategories Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		QuestCategories result = default(QuestCategories);
		result.Categories = new QuestCategory[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref QuestCategory reference = ref result.Categories[i];
			reference = QuestCategory.Unpack(unpacker);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Epic = null;
		}
		else
		{
			QuestCategory value = QuestCategory.Unpack(unpacker);
			result.Epic = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<QuestCategories Categories={Categories} Epic={Epic}>";
	}
}
