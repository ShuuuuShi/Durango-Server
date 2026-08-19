using System.Collections.Generic;
using ItemSystem;
using Messages;
using UnityEngine;

public class Cage : ArtifactComponent
{
	private const float Margin = 200f;

	private List<AnimalBehavior> _animals = new List<AnimalBehavior>();

	private List<ItemData> _reinsList = new List<ItemData>();

	public List<ItemData> ReinsList => _reinsList;

	public int RemainSize { get; private set; }

	public int Capacity { get; private set; }

	public Vector3 ClientPos => TerrainA6.TilePositionToClientPosition(base.Artifact.WorldTile);

	public Vector3 MinArea => ClientPos + new Vector3(200f, 0f, 200f);

	public Vector3 MaxArea => ClientPos + new Vector3((float)base.Artifact.Size.x, 0f, (float)base.Artifact.Size.y) * 200f - new Vector3(200f, 0f, 200f);

	public override bool OnUpdateState(double eventTime)
	{
		Set(base.Artifact.ArtifactState.Cage);
		return false;
	}

	private void Set(Messages.Cage? msg)
	{
		if (msg.HasValue)
		{
			Messages.Cage value = msg.Value;
			SetCapacity(value.CageSize, value.CageRemainSize);
			ItemData[] array = new ItemData[value.CagedReins.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ItemData(value.CagedReins[i]);
			}
			SetAnimals(array);
		}
		else
		{
			SetCapacity(0, 0);
			SetAnimals(null);
		}
	}

	private void SetCapacity(int capacity, int remainSize)
	{
		Capacity = capacity;
		RemainSize = remainSize;
	}

	private void SetAnimals(IList<ItemData> items)
	{
		ItemData[] array = _reinsList.ToArray();
		_reinsList.Clear();
		int num = items?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			ItemSystem.Reins reins = items[i].Reins;
			if (reins != null)
			{
				int num2 = Util.IndexOf(array, items[i].Id);
				if (num2 == -1)
				{
					_reinsList.Add(items[i]);
					continue;
				}
				_reinsList.Add(array[num2]);
				array[num2] = null;
			}
		}
		int j = 0;
		for (int num3 = array.Length; j < num3; j++)
		{
			if (array[j] != null)
			{
				RemoveAnimal(array[j].Id);
			}
		}
		for (int k = 0; k < _reinsList.Count; k++)
		{
			MakeAnimal(_reinsList[k]);
		}
	}

	public AnimalBehavior GetAnimal(ulong id)
	{
		int num = IndexOf(_animals, id);
		if (num == -1)
		{
			return null;
		}
		return _animals[num];
	}

	private void MakeAnimal(ItemData item)
	{
		ItemSystem.Reins reins = item.Reins;
		KSingleton<AnimalManager>.Instance().MakeCageAnimal(item.Id, reins.VehicleEntityType, reins.PetName, ((Component)base.Artifact).gameObject, OnMakeFinished);
	}

	private void RemoveAnimal(ulong id)
	{
		int num = IndexOf(_animals, id);
		if (num != -1)
		{
			AnimalBehavior animalBehavior = _animals[num];
			_animals.RemoveAt(num);
			if (!((Object)(object)animalBehavior == (Object)null))
			{
				KSingleton<AnimalManager>.Instance().RemoveAnimal(animalBehavior);
				Object.Destroy((Object)(object)((Component)animalBehavior).gameObject);
			}
		}
	}

	private void OnMakeFinished(GameObject obj)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)obj == (Object)null))
		{
			AnimalBehavior component = obj.GetComponent<AnimalBehavior>();
			_animals.Add(component);
			PetAI petAI = obj.GetComponent<PetAI>();
			if ((Object)(object)petAI == (Object)null)
			{
				petAI = obj.AddComponent<PetAI>();
			}
			petAI.Init(((Component)base.Artifact).gameObject, inCage: true, isRiding: false, MinArea, MaxArea);
			Vehicle component2 = obj.GetComponent<Vehicle>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.SetupSaddle(setupSaddle: false);
			}
		}
	}

	private static int IndexOf(IList<AnimalBehavior> list, ulong id)
	{
		int i = 0;
		for (int num = list?.Count ?? 0; i < num; i++)
		{
			ulong entityId = list[i].EntityId;
			if (entityId != 0L && entityId == id)
			{
				return i;
			}
		}
		return -1;
	}

	public void HighlightAnimal(ulong id, bool enable)
	{
		int num = IndexOf(_animals, id);
		if (num != -1)
		{
			((Component)_animals[num]).SendMessage("OnSelected", (object)enable);
		}
	}

	public override void OnRemoved()
	{
		int count = _reinsList.Count;
		for (int i = 0; i < count; i++)
		{
			RemoveAnimal(_reinsList[i].Id);
		}
		_reinsList.Clear();
		base.OnRemoved();
	}
}
