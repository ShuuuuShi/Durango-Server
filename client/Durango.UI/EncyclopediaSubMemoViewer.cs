using System.Collections;
using Durango.Logic.Encyclopedia;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class EncyclopediaSubMemoViewer : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private KScrollView _scollList;

	public MemoType MemoType { get; private set; }

	public bool IsOpen { get; private set; }

	private void Set(MemoType memoType, Submemo memo, int initMemo)
	{
		_titleLabel.text = memo.Title;
		MemoType = memoType;
		ListObjectPool nodes = _scollList.Nodes;
		nodes.Set(memo.Indexes.Length);
		BitArray activeMemoFlags = GameSystem<MemoSystem>.Instance().GetActiveMemoFlags(memoType);
		int num = -1;
		for (int i = 0; i < nodes.Count; i++)
		{
			EncyclopediaSubMemoTextNode component = nodes[i].GetComponent<EncyclopediaSubMemoTextNode>();
			float number = memo.Numbers[i];
			int num2 = memo.Indexes[i];
			bool available = num2 < activeMemoFlags.Length && activeMemoFlags[num2];
			component.Set(memoType, num2, number, available);
			if (initMemo == num2)
			{
				num = i;
			}
		}
		_scollList.ResetPosition();
		_scollList.MoveToNode((num != -1) ? num : 0, instant: true);
	}

	public void Show(MemoType type, Submemo memo, int initMemo = -1)
	{
		IsOpen = true;
		base.gameObject.SetActive(value: true);
		Set(type, memo, initMemo);
	}

	public void Hide()
	{
		IsOpen = false;
		base.gameObject.SetActive(value: false);
	}
}
