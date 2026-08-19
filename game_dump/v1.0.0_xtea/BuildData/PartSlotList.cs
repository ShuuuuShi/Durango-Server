using System.Collections.Generic;
using ItemSystem;

namespace BuildData;

public class PartSlotList
{
	private readonly List<List<ItemData>> _slotList = new List<List<ItemData>>();

	public int Count => _slotList.Count;

	public List<ItemData> this[int index] => _slotList[index];

	public PartSlotList(int slotCount)
	{
		Reset(slotCount);
	}

	public void Clear()
	{
		_slotList.Clear();
	}

	public void Reset(int slotCount)
	{
		_slotList.Clear();
		for (int i = 0; i < slotCount; i++)
		{
			_slotList.Add(new List<ItemData>());
		}
	}

	public List<ItemData> GetSlotList(int index)
	{
		return (0 > index || index >= _slotList.Count) ? null : _slotList[index];
	}
}
