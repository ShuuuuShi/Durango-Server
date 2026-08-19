using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;

namespace Durango.Logic.Statistics;

public class FavoriteTitles
{
	public const string StorageKey = "CharacterTitle";

	private static HashSet<string> _favoriteTitles = new HashSet<string>();

	public int Count => _favoriteTitles.Count;

	public void Save()
	{
		Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			SetStorageItem msg = default(SetStorageItem);
			msg.Key = "CharacterTitle";
			msg.Value = Json.WriteToBytes(_favoriteTitles);
			Connections.Frontend.Send(msg);
		});
	}

	public void Load(Dictionary<string, byte[]> storage)
	{
		HashSet<string> hashSet = Json.Read<HashSet<string>>(storage?.Get("CharacterTitle"));
		if (hashSet != null && hashSet.Count != 0)
		{
			_favoriteTitles = hashSet;
		}
	}

	public bool IsFavorite(string targetId)
	{
		return _favoriteTitles.Contains(targetId);
	}

	public void Toggle(string targetId)
	{
		if (!_favoriteTitles.Add(targetId))
		{
			_favoriteTitles.Remove(targetId);
		}
	}
}
