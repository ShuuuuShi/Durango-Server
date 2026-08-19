using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Encyclopedia;

public class FarmingEncyclopediaSystem : GameSystem<FarmingEncyclopediaSystem>
{
	private Dictionary<string, FarmingEncyclopediaData> _farmingEncyclopediaDictionary;

	public event Action<string, FarmingEncyclopediaData?, FarmingEncyclopediaData> FarmingDataUpdated;

	private void Awake()
	{
		Connections.Frontend.On<FarmingEncyclopediaProgress>(OnFarmingEncyclopediaProgress);
		Singleton<GameManager>.Instance().AddOnReady(OnReady);
	}

	private void OnReady()
	{
		Connections.Frontend.Send(new GetEncyclopedia
		{
			EncyclopediaCategory = EncyclopediaType.Farming
		}).On(delegate(FarmingEncyclopedia msg, PacketHeader header)
		{
			_farmingEncyclopediaDictionary = msg.Data;
		});
	}

	private void OnFarmingEncyclopediaProgress(FarmingEncyclopediaProgress msg, PacketHeader header)
	{
		if (_farmingEncyclopediaDictionary == null)
		{
			return;
		}
		string seedPrototypeId = msg.SeedPrototypeId;
		if (!string.IsNullOrEmpty(seedPrototypeId))
		{
			FarmingEncyclopediaData? farmingEncyclopediaData = GetFarmingEncyclopediaData(seedPrototypeId);
			_farmingEncyclopediaDictionary[seedPrototypeId] = msg.Data;
			if (this.FarmingDataUpdated != null)
			{
				this.FarmingDataUpdated(seedPrototypeId, farmingEncyclopediaData, msg.Data);
			}
		}
	}

	public IEnumerable<KeyValuePair<string, FarmingEncyclopediaData>> GetFarmingEncyclopediaDataList()
	{
		return _farmingEncyclopediaDictionary;
	}

	public FarmingEncyclopediaData? GetFarmingEncyclopediaData(string key)
	{
		if (_farmingEncyclopediaDictionary != null && !string.IsNullOrEmpty(key) && _farmingEncyclopediaDictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public static void ChangeFarmingEncyclopediaMastery(string key, int level, int mastaryIndex, bool isSelect)
	{
		Connections.Frontend.Send(new ChangeFarmingEncyclopediaMastery
		{
			SeedPrototypeId = key,
			TargetLevel = level,
			TargetMasteryIndex = mastaryIndex,
			IsSelectOrSwap = isSelect
		});
	}
}
