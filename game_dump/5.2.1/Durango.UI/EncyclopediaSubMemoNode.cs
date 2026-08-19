using System.Collections;
using Durango.Logic.Encyclopedia;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class EncyclopediaSubMemoNode : SelectableWidget
{
	[SerializeField]
	private UILabel _titleLable;

	public MemoType MemoType { get; private set; }

	public Submemo Memo { get; private set; }

	public void Set(MemoType type, Submemo memo)
	{
		Memo = memo;
		MemoType = type;
		int num = memo.Indexes.Length;
		int num2 = 0;
		BitArray activeMemoFlags = GameSystem<MemoSystem>.Instance().GetActiveMemoFlags(type);
		for (int i = 0; i < num; i++)
		{
			int num3 = memo.Indexes[i];
			if (num3 < activeMemoFlags.Length && activeMemoFlags[num3])
			{
				num2++;
			}
		}
		_titleLable.text = $"{memo.Title}    [ffd85b]{num2}[-] [71716B]/[-] [e8e5df]{num}[-]";
	}
}
