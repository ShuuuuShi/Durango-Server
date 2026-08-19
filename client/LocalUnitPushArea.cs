using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class LocalUnitPushArea
{
	private class Unit
	{
		public CharacterBehavior Target;

		public float Radius;

		public bool Process(Vector3 pos, out Vector3 dir, out float power)
		{
			if (!Target.IsAlive)
			{
				dir = Vector3.zero;
				power = 0f;
				return false;
			}
			Vector3 currentPosition = Target.CurrentPosition;
			Vector3 vector = pos - currentPosition;
			if (vector.sqrMagnitude >= Radius * Radius)
			{
				dir = Vector3.zero;
				power = 0f;
				return false;
			}
			dir = vector.normalized;
			power = vector.magnitude / Radius;
			power *= power;
			power = 1f - power;
			return true;
		}
	}

	private readonly Dictionary<string, Unit> _units = new Dictionary<string, Unit>();

	public LocalUnitPushArea()
	{
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(OnReady);
	}

	private void OnReady()
	{
		Durango.Utils.Singleton<AnimalManager>.Instance().AnimalAppeared += OnAppearAnimal;
		Durango.Utils.Singleton<AnimalManager>.Instance().AnimalDisappeared += OnDisappearAnimal;
	}

	private void OnAppearAnimal(AnimalBehavior animal)
	{
		Animal animal2 = SingletonDict<int, Animal>.Get(animal.EntityTypeId);
		if (animal2 != null)
		{
			_units[animal.EntityId] = new Unit
			{
				Target = animal,
				Radius = animal2.BoundRadius * animal.transform.localScale.x
			};
		}
	}

	private void OnDisappearAnimal(AnimalBehavior animal)
	{
		_units.Remove(animal.EntityId);
	}

	public bool ProcessUnitPush(Vector3 pos, out Vector3 dir, out float power)
	{
		dir = Vector3.zero;
		power = 0f;
		foreach (KeyValuePair<string, Unit> unit in _units)
		{
			if (unit.Value.Process(pos, out var dir2, out var power2) && power2 > power)
			{
				power = power2;
				dir = dir2;
			}
		}
		return power > 0f;
	}
}
