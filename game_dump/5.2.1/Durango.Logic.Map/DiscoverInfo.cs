using System.Collections.Generic;
using Durango.UI;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;
using Yaml;

namespace Durango.Logic.Map;

public class DiscoverInfo
{
	public const float DiscoverForAnimal = 500f;

	private Messages.DiscoveryInfo _info;

	private readonly HashSet<ushort> _discoveredEntityTypes = new HashSet<ushort>();

	private readonly List<GameObject> _objectList = new List<GameObject>();

	private float _refreshAt;

	public void Set(Messages.DiscoveryInfo discovery)
	{
		_info = discovery;
		_discoveredEntityTypes.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(discovery.AnimalTypes); i < size; i++)
		{
			if (discovery.AnimalTypes[i].Item2)
			{
				_discoveredEntityTypes.Add(discovery.AnimalTypes[i].Item1);
			}
		}
	}

	public void Process()
	{
		float time = Time.time;
		if (_refreshAt < time)
		{
			_refreshAt = time + 1f;
			SearchNearAnimals();
		}
	}

	private void SearchNearAnimals()
	{
		int size = KUtility.GetSize(_info.AnimalTypes);
		int num = 0;
		for (int i = 0; i < size; i++)
		{
			if (_info.AnimalTypes[i].Item2)
			{
				num++;
			}
		}
		if (num == size)
		{
			return;
		}
		_objectList.Clear();
		InteractionSystem.GetNearObjectsInternal(_objectList, LayerHelper.DefaultMask, 500f, AnimalFilter);
		for (int j = 0; j < _objectList.Count; j++)
		{
			AnimalBehavior component = _objectList[j].GetComponent<AnimalBehavior>();
			ushort entityType = (ushort)component.EntityTypeId;
			if (_discoveredEntityTypes.Contains(entityType))
			{
				continue;
			}
			_discoveredEntityTypes.Add(entityType);
			bool isHuman = component is HumanBehavior;
			MapSystem.DiscoverAnimalType(component.EntityId, delegate(bool succeed)
			{
				if (succeed)
				{
					UIManager.Alarm.RewardAlarm(new AlarmRewardQueue.Args
					{
						Main = AnimalYaml.GetName(entityType),
						Sub = ((!isHuman) ? T._("새로운 동물 발견") : T._("새로운 적 발견")),
						Icon = AnimalYaml.GetPortrait(entityType)
					}, AlarmGroup.RewardEffectType.AnimalDiscovered);
				}
				else
				{
					_discoveredEntityTypes.Remove(entityType);
				}
			});
		}
	}

	private static GameObject AnimalFilter(GameObject obj)
	{
		if ((bool)obj && obj.CompareTag("Enemy") && (bool)obj.GetComponent<AnimalBehavior>() && obj.GetComponent<PetAI>() == null)
		{
			return obj;
		}
		return null;
	}
}
