using System.Collections.Generic;

public class ModularAddons
{
	private List<ModularAddon> _addons = new List<ModularAddon>();

	public void Set(Dictionary<int, KeyValuePair<string, string>> addons)
	{
		_addons.Clear();
		if (addons == null)
		{
			return;
		}
		foreach (KeyValuePair<int, KeyValuePair<string, string>> addon in addons)
		{
			_addons.Add(new ModularAddon
			{
				Index = addon.Key,
				ModelKey = addon.Value.Key,
				Type = addon.Value.Value
			});
		}
	}

	public void Set(ModularAddons addons)
	{
		_addons.Clear();
		if (addons != null)
		{
			int i = 0;
			for (int count = addons._addons.Count; i < count; i++)
			{
				ModularAddon modularAddon = addons._addons[i];
				_addons.Add(new ModularAddon
				{
					Index = modularAddon.Index,
					ModelKey = modularAddon.ModelKey,
					Type = modularAddon.Type,
					Item = modularAddon.Item
				});
			}
		}
	}

	private int IndexOf(int index)
	{
		int i = 0;
		for (int count = _addons.Count; i < count; i++)
		{
			if (_addons[i].Index == index)
			{
				return i;
			}
		}
		return -1;
	}

	public ModularAddon Get(int index)
	{
		int num = IndexOf(index);
		if (num == -1)
		{
			return null;
		}
		return _addons[num];
	}

	public ModularAddon Set(int index, ModularAddon addon)
	{
		int num = IndexOf(index);
		if (num == -1)
		{
			_addons.Add(new ModularAddon
			{
				Index = index,
				ModelKey = addon.ModelKey,
				Type = addon.Type,
				Item = addon.Item
			});
			return null;
		}
		ModularAddon modularAddon = new ModularAddon();
		modularAddon.Index = _addons[num].Index;
		modularAddon.Item = _addons[num].Item;
		modularAddon.ModelKey = _addons[num].ModelKey;
		modularAddon.Type = _addons[num].Type;
		ModularAddon result = modularAddon;
		_addons[num].ModelKey = addon.ModelKey;
		_addons[num].Type = addon.Type;
		_addons[num].Item = addon.Item;
		return result;
	}

	public void Move(int from, int to)
	{
		int num = IndexOf(from);
		int num2 = IndexOf(to);
		if (num != -1)
		{
			_addons[num].Index = to;
			if (num2 != -1)
			{
				_addons[num2].Index = from;
			}
		}
	}

	public void Remove(int index)
	{
		int num = IndexOf(index);
		if (num != -1)
		{
			_addons.RemoveAt(num);
		}
	}

	public Dictionary<int, ulong> GetAddonIds()
	{
		Dictionary<int, ulong> dictionary = new Dictionary<int, ulong>();
		int i = 0;
		for (int count = _addons.Count; i < count; i++)
		{
			if (_addons[i].Item == null)
			{
				return null;
			}
			dictionary.Add(_addons[i].Index, _addons[i].Item.Id);
		}
		return dictionary;
	}
}
